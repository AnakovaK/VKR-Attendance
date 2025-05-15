using System.Security.Claims;

using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using RTUAttendAPI.API.Authorization.Entities;
using RTUAttendAPI.AttendDatabase.Models;

namespace RTUAttendAPI.API.Extensions;

public static class AuthenticateExtensions
{
    public static Guid GetHumanId(this ServerCallContext context)
        => context.GetHttpContext().User.GetUserId();
    public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userIdValue = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new NoUserIdException();
        return Guid.TryParse(userIdValue, out var id) ? id : throw new UserIdIsNotGuidException(userIdValue);
    }
    public static ClaimsPrincipal GetUser(this ServerCallContext context) => context.GetHttpContext().User;

    public static async Task RpcAuthorizeToVisitingLogRead(this IAuthorizationService authService, ServerCallContext context, VisitingLogId visitingLogId)
    {
        var authorizeInfo = await authService.AuthorizeAsync(context.GetUser(), visitingLogId, VisitingLogOperations.Read);

        if (!authorizeInfo.Succeeded)
        {
            throw new RpcException(new Status(StatusCode.PermissionDenied, "does not have access to visiting log"));
        }
    }
}
public class NoUserIdException : Exception
{
}
public class UserIdIsNotGuidException : Exception
{
    public UserIdIsNotGuidException(string userIdValue) : base($"User id {userIdValue} is not guid") { }
}

