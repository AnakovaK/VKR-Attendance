using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json.Nodes;
using System.Text.Json;
using AttendDatabase;
using Microsoft.EntityFrameworkCore;
using RtuTc.TandemSchedule;
using RTUAttendAPI.API.Services.Initialization;
using Microsoft.AspNetCore.WebUtilities;

namespace RTUAttendAPI.API.Authorization.MIREA;


public class MIREAHandler : OAuthHandler<MIREAOptions>
{
    private readonly AttendDbContext _attendDbContext;
    private readonly IHumanLoginInfoService _humanInfoService;

    public MIREAHandler(
        IOptionsMonitor<MIREAOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        AttendDbContext attendDbContext,
        IHumanLoginInfoService humanInfoService)
        : base(options, logger, encoder, clock)
    {
        _attendDbContext = attendDbContext;
        _humanInfoService = humanInfoService;
    }

    public override Task<bool> HandleRequestAsync()
    {
        Events.OnRemoteFailure = HandleRemoteFailure;
        return base.HandleRequestAsync();
    }

    protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
    {
        try
        {
            return await CustomCreateTicket(identity, properties, tokens);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Can't create token for user with token {AccessToken}", tokens.AccessToken);
            return null!;
        }
    }
    private async Task<AuthenticationTicket> CustomCreateTicket(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
    {
        var userInfo = await GetInfoFromBaseProcess(tokens);
        Logger.LogInformation("Received user info {UserInfo}", userInfo);
        if (userInfo is null)
        {
            Logger.LogError("login system not provide uid field");
            return null!;
        }

        var entry = _attendDbContext.LoginEvents.Add(new AttendDatabase.Models.LoginEvent
        {
            Date = DateTimeOffset.UtcNow,
            LoginInfo = JsonDocument.Parse(JsonSerializer.Serialize(userInfo))
        });
        await _attendDbContext.SaveChangesAsync();
        entry.State = EntityState.Detached;

        if (!userInfo.TryGetPropertyValue("uid", out var uid))
        {
            Logger.LogCritical("login system not provide uid field");
            return null!;
        }
        var uidFromLogin = Guid.Parse(uid!.GetValue<string>());

        var humanInfo = await _humanInfoService.GetHumanLoginInfoByLogin(uidFromLogin, Context.RequestAborted);

        userInfo.Remove("uid");
        userInfo.Add("uid", JsonNode.Parse($"\"{humanInfo.HumanId}\""));

        using var jsonDoc = JsonDocument.Parse(userInfo.ToJsonString());

        var principal = new ClaimsPrincipal(identity);
        var context = new OAuthCreatingTicketContext(principal, properties, Context, Scheme, Options, Backchannel, tokens, jsonDoc.RootElement);

        context.RunClaimActions();

        if (humanInfo.IsTeacher)
        {
            identity.AddClaim(new Claim("role", "teacher", null, "attendance"));
        }

        identity.AddClaim(new Claim("iss", MIREADefaults.AuthenticationScheme));
        identity.AddClaim(new Claim("role", "admin", null, "attendance"));

        await Events.CreatingTicket(context);

        return new AuthenticationTicket(principal, context.Properties, Scheme.Name);
    }

    private async Task<JsonObject> GetInfoFromBaseProcess(OAuthTokenResponse tokens)
    {
        var userInfo = await GetUserInfo(Options.UserInformationEndpoint, tokens.AccessToken, Context.RequestAborted)
            ?? throw new Exception($"Can't get info about user with token {tokens.AccessToken}");
        return userInfo;
    }

    private Task HandleRemoteFailure(RemoteFailureContext failureContext)
    {
        Logger.LogWarning(failureContext.Failure, "Remote failure");
        failureContext.HandleResponse();
        // TODO: разделить отмену разрешения и ошибку
        var uri = failureContext.Options.AccessDeniedPath.ToString();
        if (!string.IsNullOrEmpty(failureContext.Options.ReturnUrlParameter) && !string.IsNullOrEmpty(failureContext.Properties?.RedirectUri))
        {
            uri = QueryHelpers.AddQueryString(uri, failureContext.Options.ReturnUrlParameter, failureContext.Properties.RedirectUri);
        }
        Response.Redirect(BuildRedirectUri(uri));
        return Task.CompletedTask;
    }

    private async Task<JsonObject?> GetUserInfo(string endpoint, string? accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        using var response = await Backchannel.SendAsync(request, Context.RequestAborted);
        if (!response.IsSuccessStatusCode)
        {
            Logger.LogDebug("{StatusCode} on request to {EndPoint}", response.StatusCode, endpoint);
            return null;
        }
        return JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken))!.AsObject();
    }
}
