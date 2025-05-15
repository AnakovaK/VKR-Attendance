using Ardalis.Specification.EntityFrameworkCore;

using AttendDatabase;

using Grpc.Core;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using Quartz;

using RTUAttendAPI.API.Constants;
using RTUAttendAPI.API.Extensions;
using RTUAttendAPI.AttendDatabase.Specs;

using RtuTc.RtuAttend.App;

using static RtuTc.RtuAttend.App.AdminService;

namespace RTUAttendAPI.API.Services.Grpc;

internal class AdminService : AdminServiceBase
{
    private readonly AttendDbContext _attendDbContext;
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        AttendDbContext attendDbContext, // используется для обновления TODO: разнести выборки и изменения в разные grpc сервисы (мини CQRS)
        ISchedulerFactory schedulerFactory,
        IAuthorizationService authorizationService,
        ILogger<AdminService> logger
    )
    {
        _attendDbContext = attendDbContext;
        _schedulerFactory = schedulerFactory;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public override async Task<UpdateStudentMembershipRoleResponse> UpdateStudentMembershipRole(UpdateStudentMembershipRoleRequest request, ServerCallContext context)
    {
        var isAdmin = (await _authorizationService.AuthorizeAsync(context.GetUser(), "Admin")).Succeeded;

        if (!isAdmin)
        {
            _logger.LogInformation("User by human Id {UserId} is not admin", context.GetHumanId());
        }

        var studentToUpdate = _attendDbContext.StudentMemberships.WithSpecification(new GetStudentByMembershipIdSpec(request.StudentMembershipId.AsGuid()));
        await studentToUpdate.ExecuteUpdateAsync(sm => sm.SetProperty(s => s.MembershipRole, request.NewRole.FromGrpc()));
        return new UpdateStudentMembershipRoleResponse();
    }

    public override async Task<GetStudentMembershipIdByVisitingLogIdStudentIdResponse> GetStudentMembershipIdByVisitingLogIdStudentId(GetStudentMembershipIdByVisitingLogIdStudentIdRequest request, ServerCallContext context)
    {
        var isAdmin = (await _authorizationService.AuthorizeAsync(context.GetUser(), "Admin")).Succeeded;

        if (!isAdmin)
        {
            _logger.LogInformation("User by human Id {UserId} is not admin", context.GetHumanId());
        }
        var studentMembership = await _attendDbContext.StudentMemberships.WithSpecification(new GetStudentMembershipByIdsSpec(request.VisitingLogId.AsGuid(), request.StudentId.AsGuid())).SingleOrDefaultAsync(context.CancellationToken);
        if (studentMembership is null)
        {
            _logger.LogInformation("Couldn't fetch student membership with visiting log id {VisitingLogId} student id {StudentId}", request.VisitingLogId.AsGuid(), request.StudentId.AsGuid());
            throw new Exception("Membership was not found");
        }
        return new GetStudentMembershipIdByVisitingLogIdStudentIdResponse()
        {
            StudentMembershipId = studentMembership.Id.ToString()
        };
    }

    public override async Task<TriggerGetAllAcademicGroupsResponse> TriggerGetAllAcademicGroups(TriggerGetAllAcademicGroupsRequest request, ServerCallContext context)
    {
        var isAdmin = (await _authorizationService.AuthorizeAsync(context.GetUser(), "Admin")).Succeeded;

        if (!isAdmin)
        {
            _logger.LogInformation("User by human Id {UserId} is not admin", context.GetHumanId());
        }
        var scheduler = await _schedulerFactory.GetScheduler();
        await scheduler.TriggerJob(JobKeys.ACADEMIC_GROUPS_FETCH, context.CancellationToken);
        return new TriggerGetAllAcademicGroupsResponse();
    }
}
