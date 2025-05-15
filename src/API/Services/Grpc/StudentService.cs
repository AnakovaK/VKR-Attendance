using AttendDatabase;

using Grpc.Core;

using Microsoft.AspNetCore.Authorization;

using RTUAttendAPI.API.Extensions;
using RTUAttendAPI.API.Services.BusinessLogic;

using RtuTc.RtuAttend.App;

using static RtuTc.RtuAttend.App.StudentService;

namespace RTUAttendAPI.API.Services.Grpc;

[Authorize]
public class StudentService : StudentServiceBase
{
    private readonly IStudentSelfApproveLinksService _studentSelfApproveLinksService;
    private readonly AttendanceService _attendanceService;

    public StudentService(IStudentSelfApproveLinksService studentSelfApproveLinksService, AttendanceService attendanceService)
    {
        _studentSelfApproveLinksService = studentSelfApproveLinksService;
        _attendanceService = attendanceService;
    }
    public override async Task<SelfApproveAttendanceResponse> SelfApproveAttendance(SelfApproveAttendanceRequest request, ServerCallContext context)
    {
        var targetLessonId = await _studentSelfApproveLinksService.GetLessonIdForToken(request.Token, context.CancellationToken);
        if (!targetLessonId.HasValue)
        {
            return new SelfApproveAttendanceResponse
            {
                NotYet = new NotYetApprovedResponse()
                {
                    Reason = NotYetApprovedReason.IncorrectToken,
                }
            };
        }
        var setAttendaceResult = await _attendanceService.SetAttendanceToPresentForStudent(context.GetHumanId(), targetLessonId.Value, context.CancellationToken);
        if (setAttendaceResult.IsT0)
        {
            return new SelfApproveAttendanceResponse
            {
                NotYet = new NotYetApprovedResponse
                {
                    Reason = setAttendaceResult.AsT0 switch
                    {
                        AttendanceService.SetAttendanceToPresentForStudentError.NoLinkToLesson => NotYetApprovedReason.NoLinkToLesson,
                        _ => NotYetApprovedReason.Unknown,
                    }
                }
            };
        }
        return new SelfApproveAttendanceResponse
        {
            Approved = GrpcMapper.Map(setAttendaceResult.AsT1)
        };
    }
}
