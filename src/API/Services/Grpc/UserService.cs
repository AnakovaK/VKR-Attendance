using System.Security.Claims;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

using RTUAttendAPI.API.Authorization;
using RTUAttendAPI.API.Configuration;

using RtuTc.RtuAttend.App;

using static RtuTc.RtuAttend.App.UserService;

namespace RTUAttendAPI.API.Services.Grpc;

internal class UserService : UserServiceBase
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IOptions<PublicOriginOptions> _options;

    public UserService(LinkGenerator linkGenerator, IOptions<PublicOriginOptions> options)
    {
        _linkGenerator = linkGenerator;
        _options = options;
    }
    public override async Task<GetMeInfoResponse> GetMeInfo(GetMeInfoRequest request, ServerCallContext context)
    {
        var authInfo = await context.GetHttpContext().AuthenticateAsync();
        if (!authInfo.Succeeded)
        {
            return new GetMeInfoResponse
            {
                NotAuthorized = new NotAuthorizedInfo
                {
                    LoginUrl = _options.Value.Uri + _linkGenerator.GetPathByName("login", new
                    {
                        redirectUri = request.AppUrl,
                    }),
                },
            };
        }
        var user = authInfo.Principal!;
        var authorized = new AuthorizedInfo
        {
            LogoutUrl = _options.Value.Uri + _linkGenerator.GetPathByName("logout", new
            {
                redirectUri = request.AppUrl,
            }),
            User = new UserInfo
            {
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
                Lastname = user.FindFirstValue(ClaimTypes.Surname) ?? "surname",
                Firstname = user.FindFirstValue(ClaimTypes.GivenName) ?? "lastname",
                Middlename = user.FindFirstValue(SpecialClaims.Patronymic) ?? "middlename",
            },
        };
        authorized.User.Claims.AddRange(user.Claims.Where(c => !DirectClaims.Contains(c.Type)).Select(c => new RtuTc.RtuAttend.App.Claim
        {
            Type = c.Type,
            Value = c.Value
        }));
        return new GetMeInfoResponse
        {
            Authorized = authorized,
        };
    }
    private static readonly string[] DirectClaims = new string[] { ClaimTypes.NameIdentifier, ClaimTypes.Name, ClaimTypes.Surname, ClaimTypes.GivenName, SpecialClaims.Patronymic };
}
