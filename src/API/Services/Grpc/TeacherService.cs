using System.Diagnostics;

using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

using AttendDatabase;

using global::Grpc.Core;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using RTUAttendAPI.API.Authorization.Entities;
using RTUAttendAPI.API.Extensions;
using RTUAttendAPI.API.Services.BusinessLogic;
using RTUAttendAPI.AttendDatabase.Models;
using RTUAttendAPI.AttendDatabase.Specs;

using RtuTc.RtuAttend.App;
using RtuTc.RtuAttend.Models;

using static RtuTc.RtuAttend.App.TeacherService;

namespace RTUAttendAPI.API.Services.Grpc;

[Authorize(Policy = "Teacher")]
public class TeacherService : TeacherServiceBase
{
    private readonly AttendDbContext _attendDbContext;
    private readonly AttendanceService _attendanceService;
    private readonly IStudentSelfApproveLinksService _selfApproveLinksService;
    private readonly ILogger<TeacherService> _logger;
    public TeacherService(
        AttendDbContext attendDbContext,
        AttendanceService attendanceService,
        IStudentSelfApproveLinksService selfApproveLinksService,
        ILogger<TeacherService> logger)
    {
        _attendDbContext = attendDbContext;
        _attendanceService = attendanceService;
        _selfApproveLinksService = selfApproveLinksService;
        _logger = logger;
    }
    // TODO: роль учителя
    public override async Task<GetAllSemestersResponse> GetAllSemesters(GetAllSemestersRequest request, ServerCallContext context)
    {
        var result = new GetAllSemestersResponse();

        var allSemesters = await _attendDbContext.Semesters
            .WithSpecification(new GetAllSemesetersForTeacherSpec())
            .ToListAsync(context.CancellationToken);

        result.Semesters.AddRange(allSemesters.Select(s => new Semesters
        {
            Id = s.Id.ToString(),
            Title = s.Title,
            Start = s.Start.ToTimestamp(),
            End = s.End.ToTimestamp(),
        }));

        return result;
    }
    public override async Task<GetAllVisitingLogsResponse> GetAllVisitingLogs(GetAllVisitingLogsRequest request, ServerCallContext context)
    {
        var result = new GetAllVisitingLogsResponse();

        var semesterId = request.SemesterId.AsGuid();

        var visitingLogsOfSemester = await _attendDbContext.VisitingLogs
            .WithSpecification(new GetVisitingLogsOfSemesterForTeacherSpec(semesterId))
            .ToListAsync(context.CancellationToken);

        result.VisitingLogs.AddRange(visitingLogsOfSemester.Select(vl => new VisitingLogs
        {
            Id = vl.Id.ToString(),
            Title = vl.Title,
            IsActive = !vl.IsArchieved,
            StudentsCount = vl.StudentsCount,
        }));

        return result;
    }
    public override async Task<GetMyLessonsForDateResponse> GetMyLessonsForDate(GetMyLessonsForDateRequest request, ServerCallContext context)
    {
        var lessonsForDay = await _attendDbContext.Lessons
            .WithSpecification(new GetLessonsForTeacherDaySpec(context.GetHumanId(), request.Date.ToDateOnly()))
            .ToListAsync(context.CancellationToken);

        var result = new GetMyLessonsForDateResponse();
        foreach (var lesson in lessonsForDay)
        {
            result.Lessons.Add(GrpcMapper.Map(lesson));
        }
        return result;
    }

    public override async Task<GetAttendancesForTeacherResponse> GetAttendancesForTeacher(GetAttendancesForTeacherRequest request, ServerCallContext context)
    {
        var lessonId = request.LessonId.AsGuid();
        var visitingLogs = await _attendDbContext.Lessons.WithSpecification(new GetVisitingLogsForLesson(lessonId))
            .ToListAsync();
        var currentLesson = await _attendDbContext.Lessons.WithSpecification(new GetLessonsByIdSpec(lessonId)).SingleOrDefaultAsync();
        if (currentLesson == default)
        {
            _logger.LogWarning("Lesson by Id {lessonId} was not found", lessonId);
            throw new Exception("Lesson by id was not found");
        }
        var result = new GetAttendancesForTeacherResponse();
        foreach (var log in visitingLogs)
        {
            var record = new AttendancesForVisitingLog
            {
                VisitingLog = new VisitingLogCompactInfo
                {
                    Id = log.Id.ToString(),
                    Title = log.Title
                },
            };
            var attendaces = await _attendanceService.GetAttendanceForLesson(log.Id, lessonId, context.CancellationToken);
            record.Attendances.AddRange(attendaces);
            result.AttendancesForLogs.Add(record);
        }
        result.Lesson = GrpcMapper.Map(currentLesson);
        return result;
    }

    public override async Task<ApproveLessonAttendancesResponse> ApproveLessonAttendances(ApproveLessonAttendancesRequest request, ServerCallContext context)
    {
        var lessonId = request.LessonId.AsGuid();
        await _attendanceService.ReplaceAttendUnknownStatusWithAbsent(lessonId, AuthorType.Teacher, context.GetHumanId(), context.CancellationToken);
        var updated = await _attendDbContext.Lessons
            .Where(l => l.Id == lessonId)
            .ExecuteUpdateAsync(lesson => lesson
                .SetProperty(l => l.IsValidated, l => true));
        return new ApproveLessonAttendancesResponse();
    }

    public override async Task GetStudentSelfApproveLink(
        GetStudentSelfApproveLinkRequest request,
        IServerStreamWriter<GetStudentSelfApproveLinkResponse> responseStream,
        ServerCallContext context)
    {
        var lessonId = request.LessonId.AsGuid();
        try
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var link = await _selfApproveLinksService.CreateLinkForLesson(lessonId, context.CancellationToken);

                var attendancesCount = await _attendanceService.GetStudentAttendancesCountForLesson(lessonId, context.CancellationToken)
                    ?? throw new UnreachableException($"Can't calculate counts for lesson {lessonId}");
                await responseStream.WriteAsync(new GetStudentSelfApproveLinkResponse
                {
                    Link = link,
                    StudentsPresentCount = attendancesCount.StudentsPresent,
                    StudentsTotalCount = attendancesCount.StudentsTotalCount
                });
                await _selfApproveLinksService.WaitForNextLink(context.CancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Done creating qr codes for {LessonId}", lessonId);
        }
    }

    public override async Task<GetVisitingLogByIdResponse> GetVisitingLogById(
        GetVisitingLogByIdRequest request,
        ServerCallContext context)
    {
        var visitingLog = await _attendDbContext.VisitingLogs
            .WithSpecification(new GetVisitingLogByIdSpec(request.VisitingLogId.AsGuid())).SingleOrDefaultAsync();
        if (visitingLog == default)
        {
            _logger.LogInformation("visiting log by id {VisitingLogId} was not found", request.VisitingLogId);
        }
        var result = new GetVisitingLogByIdResponse();
        result.IsArchived = visitingLog!.IsArchived;
        result.Title = visitingLog.Title;
        result.VisitingLogId = visitingLog.Id.ToString();
        return result;
    }

    public override async Task<GetStudentMembershipsOfVisitingLogByIdResponse> GetStudentMembershipsOfVisitingLogById(GetStudentMembershipsOfVisitingLogByIdRequest request, ServerCallContext context)
    {
        var result = new GetStudentMembershipsOfVisitingLogByIdResponse();
        var studentMemberships = await _attendDbContext.StudentMemberships
            .WithSpecification(new GetStudentMembershipsByVisitingLogIdSpec(request.VisitingLogId.AsGuid())).ToListAsync();
        result.StudentMemberships.AddRange(studentMemberships.Select(sm => new VisitingLogCompositionMembership
        {
            StudentId = sm.NsiStudentId.ToString(),
            Firstname = sm.Firstname,
            Lastname = sm.Lastname,
            Middlename = sm.Middlename,
            MembershipRole = sm.MembershipRole.ToModelGrpc(),
            MembershipType = sm.MembershipType.ToModelGrpc()
        }));
        return result;
    }
    public override async Task<SearchLessonResponse> SearchLesson(SearchLessonRequest request, ServerCallContext context)
    {
        var lessons = await _attendDbContext.Lessons
            .WithSpecification(new SearchLessonsByMatchSpec(request.Match, request.Date.ToDateOnly()))
            .ToListAsync(context.CancellationToken);
        var response = new SearchLessonResponse();
        response.Lessons.AddRange(lessons.Select(GrpcMapper.Map));
        return response;
    }

    public override async Task<AssignSelfToLessonResponse> AssignSelfToLesson(AssignSelfToLessonRequest request, ServerCallContext context)
    {
        var lessonId = request.LessonId.AsGuid();
        var lesson = await _attendDbContext.Lessons
            .WithSpecification(new GetLessonsByIdSpec(lessonId))
            .SingleOrDefaultAsync(context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "lesson not found"));
        var user = context.GetUser();
        var humanId = user.GetUserId();

        if (lesson.Teachers.Any(t => t.Id == humanId))
        {
            return new AssignSelfToLessonResponse
            {
                Status = AssignSelfToLessonStatus.AlreadyAssigned
            };
        }
        var teacherId = await _attendDbContext.Teachers
            .WithSpecification(new GetTeacherIdByHumanId(humanId))
            .SingleOrDefaultAsync(context.CancellationToken);
        if (teacherId == default)
        {
            throw new UnreachableException($"authorized as teacher but teacher entity not found for human {humanId}");
        }
        var link = new TeacherLesson
        {
            LessonId = lessonId,
            TeacherId = teacherId
        };
        _attendDbContext.TeacherLessons.Add(link);
        await _attendDbContext.SaveChangesAsync();
        return new AssignSelfToLessonResponse
        {
            Status = AssignSelfToLessonStatus.Ok
        };
    }

    public override async Task<UnassignSelfFromLessonResponse> UnassignSelfFromLesson(UnassignSelfFromLessonRequest request, ServerCallContext context)
    {
        var lessonId = request.LessonId.AsGuid();
        var lesson = await _attendDbContext.Lessons
            .WithSpecification(new GetLessonsByIdSpec(lessonId))
            .SingleOrDefaultAsync(context.CancellationToken)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "lesson not found"));
        var user = context.GetUser();
        var humanId = user.GetUserId();
        if (!lesson.Teachers.Any(t => t.Id == humanId))
        {
            return new UnassignSelfFromLessonResponse
            {
                Status = UnassignSelfFromLessonStatus.WasNotAssigned
            };
        }
        var teacherId = await _attendDbContext.Teachers
            .WithSpecification(new GetTeacherIdByHumanId(humanId))
            .SingleOrDefaultAsync(context.CancellationToken);
        if (teacherId == default)
        {
            throw new UnreachableException($"authorized as teacher but teacher entity not found for human {humanId}");
        }
        var teacherLessonLink = await _attendDbContext.TeacherLessons
            .WithSpecification(new GetTeacherLessonLinkByIds(teacherId, lessonId))
            .SingleOrDefaultAsync(context.CancellationToken);
        if (teacherLessonLink is null)
        {
            throw new UnreachableException($"Link of teacher to lesson lost, lesson {lessonId}, teacher {teacherId}");
        }
        _attendDbContext.TeacherLessons.Remove(teacherLessonLink);
        await _attendDbContext.SaveChangesAsync();
        return new UnassignSelfFromLessonResponse
        {
            Status = UnassignSelfFromLessonStatus.Ok
        };
    }
}
