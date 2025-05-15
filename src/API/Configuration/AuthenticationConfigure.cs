using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using RTUAttendAPI.API.Authorization;
using RTUAttendAPI.API.Authorization.Entities;
using RTUAttendAPI.API.Authorization.MIREA;
using RTUAttendAPI.API.Services.Initialization;

namespace RTUAttendAPI.API.Configuration;
public static class AuthenticationConfigure
{
    public static void ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        var authBuilder = builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = MIREADefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
        });
        if (string.IsNullOrEmpty(builder.Configuration.GetValue<string>("Oauth:MIREA:ClientId")))
        {
            authBuilder.AddScheme<StaticMireaSchemeOptions, StaticMireaSchemeAuthenticationHandler>(
                MIREADefaults.AuthenticationScheme,
                MIREADefaults.DisplayName,
                config =>
                {
                    builder.Configuration.GetSection("Oauth:MIREAStatic").Bind(config);
                });
        }
        else
        {
            authBuilder.AddMireaOauth(builder.Configuration.GetSection("Oauth:MIREA"));
            builder.Services.AddScoped<IHumanLoginInfoService, ReadFromDbOrCreateFromTandemHumanService>();
        }
        
        builder.Services.AddAuthorization(auth =>
        {
            auth.AddPolicy("Teacher", policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim("role", "teacher"));
        });
        builder.Services.AddAuthorization(auth =>
        {
            auth.AddPolicy("Admin", policy => policy
                .RequireAuthenticatedUser()
                .RequireClaim("role", "admin"));
        });

        builder.Services.AddScoped<IAuthorizationHandler, VisitingLogOperationsAuthorizationHandler>();


    }
}
public class DefaultClaim
{
    public required string Type { get; set; }
    public required string Value { get; set; }
}
internal class StaticMireaSchemeOptions : AuthenticationSchemeOptions
{
    public DefaultClaim[] Claims { get; set; } = Array.Empty<DefaultClaim>();
}
/// <summary>
/// Заглушка для авторизации, используется при локальной разработке
/// </summary>
internal class StaticMireaSchemeAuthenticationHandler : AuthenticationHandler<StaticMireaSchemeOptions>
{
    public StaticMireaSchemeAuthenticationHandler(
        IOptionsMonitor<StaticMireaSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        if (Request.Method == HttpMethod.Get.Method)
        {
            var claimsPrincipal = GetPrincipalFromConfig(Request.Headers.TryGetValue("human-id", out var humanId) ? humanId.ToString() : null);
            await Context.SignInAsync(claimsPrincipal);
            if (properties.RedirectUri is not null)
            {
                Response.StatusCode = 302;
                Response.Headers.Location = properties.RedirectUri;
            }
            else
            {
                Response.StatusCode = 200;
            }
        }
        else
        {
            Response.StatusCode = 401;
        }
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claimsPrincipal = GetPrincipalFromConfig(null);
        var ticket = new AuthenticationTicket(claimsPrincipal, MIREADefaults.AuthenticationScheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    private ClaimsPrincipal GetPrincipalFromConfig(string? humanId)
    {
        var claims = Options.Claims.Select(c => new Claim(c.Type, c.Type == ClaimTypes.NameIdentifier ? humanId ?? c.Value : c.Value));
        var identity = new ClaimsIdentity(claims, MIREADefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        return claimsPrincipal;
    }
}
