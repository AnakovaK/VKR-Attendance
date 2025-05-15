using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization;

using RTUAttendAPI.API.Extensions;

using Sentry;
using AttendDatabase;
using Ardalis.Specification.EntityFrameworkCore;
using Ardalis.Specification;
using RTUAttendAPI.AttendDatabase.Models;
using RTUAttendAPI.AttendDatabase.Specs;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.EntityFrameworkCore;

namespace RTUAttendAPI.API.Authorization.Entities;

public static class VisitingLogOperations
{
    public static OperationAuthorizationRequirement Read { get; } =
        new OperationAuthorizationRequirement { Name = nameof(Read) };
}


public class VisitingLogId
{
    public Guid Value { get; }
    public VisitingLogId(string value)
    {
        Value = value.AsGuid();
    }
}

public class VisitingLogOperationsAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, VisitingLogId>
{
    private readonly AttendDbContext _attendDbContext;

    public VisitingLogOperationsAuthorizationHandler(AttendDbContext attendDbContext)
    {
        _attendDbContext = attendDbContext;
    }
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, VisitingLogId resource)
    {
        if (requirement == VisitingLogOperations.Read)
        {
            if (context.User.Claims.Any(c => c.Type == "role" && c.Value == "teacher")) // TODO: вынести клэймы в правила
            {
                context.Succeed(requirement);
                return;
            }
            var hasAccess = await _attendDbContext.StudentMemberships
                .WithSpecification(new HumanAccessToReadVisitingLogSpec(context.User.GetUserId(), resource))
                .AnyAsync();
            if (hasAccess)
            {
                context.Succeed(requirement);
            }
        }
    }
}

public class HumanAccessToReadVisitingLogSpec : SingleResultSpecification<StudentMembership>
{
    public HumanAccessToReadVisitingLogSpec(Guid humanId, VisitingLogId visitingLogId)
    {
        Query
            .Where(sm => sm.MembershipType == StudentMembershipType.Active)
            .Where(sm => sm.VisitingLogId == visitingLogId.Value)
            .Where(sm => sm.NsiStudent!.NsiHumanId == humanId);
    }
}
