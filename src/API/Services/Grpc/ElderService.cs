using Ardalis.Specification;

using AttendDatabase;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Authorization;

using RTUAttendAPI.API.Authorization.Entities;
using RTUAttendAPI.API.Extensions;
using RTUAttendAPI.API.Services.BackgroundSchedule;
using RTUAttendAPI.API.Services.BusinessLogic;
using RTUAttendAPI.AttendDatabase.Models;
using RTUAttendAPI.AttendDatabase.Specs;

using RtuTc.RtuAttend.App;
using RtuTc.RtuAttend.Models;

using static RtuTc.RtuAttend.App.ElderService;

namespace RTUAttendAPI.API.Services.Grpc;

[Authorize]
internal class ElderService : ElderServiceBase
{
    private readonly IRepositoryBase<VisitingLog> _visitingLogRep;
    private readonly IRepositoryBase<StudentMembership> _studentMembershipRep;
    private readonly IRepositoryBase<Discipline> _disciplineRep;
    private readonly IRepositoryBase<Lesson> _lessonRep;
    private readonly IRepositoryBase<Attendance> _attendanceRep;
    private readonly IRepositoryBase<TimeSlot> _timeSlotRep;
    private readonly IRepositoryBase<LessonType> _lessonTypeRep;
    private readonly AttendDbContext _attendDbContext;
    private readonly AttendanceService _attendanceService;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ElderService> _logger;

    public ElderService(
        IRepositoryBase<VisitingLog> visitingLogRep,
        IRepositoryBase<StudentMembership> studentMembershipRep,
        IRepositoryBase<Discipline> disciplineRep,
        IRepositoryBase<Lesson> lessonRep,
        IRepositoryBase<Attendance> attendanceRep,
        IRepositoryBase<TimeSlot> timeSlotRep,
        IRepositoryBase<LessonType> lessonTypeRep,
        AttendDbContext attendDbContext, // используется для обновления TODO: разнести выборки и изменения в разные grpc сервисы (мини CQRS)
        AttendanceService attendanceService,
        IBackgroundJobScheduler backgroundJobScheduler,
        IAuthorizationService authorizationService,
        ILogger<ElderService> logger
    )
    {
        _visitingLogRep = visitingLogRep;
        _studentMembershipRep = studentMembershipRep;
        _disciplineRep = disciplineRep;
        _lessonRep = lessonRep;
        _attendanceRep = attendanceRep;
        _timeSlotRep = timeSlotRep;
        _lessonTypeRep = lessonTypeRep;
        _attendDbContext = attendDbContext;
        _attendanceService = attendanceService;
        _backgroundJobScheduler = backgroundJobScheduler;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    public override async Task<AvailableVisitingLogsResponse> GetAvailableVisitingLogs(GetAvailableVisitingLogsRequest request, ServerCallContext context)
    {
        var logs = await _visitingLogRep.ListAsync(new AvailableVisitingLogsForStudentSpec(context.GetHumanId()), context.CancellationToken);

        var result = new AvailableVisitingLogsResponse();
        result.Logs.AddRange(logs.Select(l => new ElderVisitingLog
        {
            Id = l.VisitingLogId.ToString(),
            Title = l.Title,
            IsActive = l.IsActive,
            StudentsCount = l.StudentsCount
        }));

        return result;
    }

    public override async Task<GroupCompositionResponse> GetGroupComposition(GetGroupCompositionRequest request, ServerCallContext context)
    {
        var isAdmin = (await _authorizationService.AuthorizeAsync(context.GetUser(), "Admin")).Succeeded;

        var visitingLogId = new VisitingLogId(request.VisitingLogId);

        if (!isAdmin)
        {
            await _authorizationService.RpcAuthorizeToVisitingLogRead(context, visitingLogId);
        }

        var result = new GroupCompositionResponse();

        var memberships = await _studentMembershipRep.ListAsync(new GroupCompositionForElderSpec(visitingLogId.Value), context.CancellationToken);
        var visitingLogData = await _visitingLogRep.SingleOrDefaultAsync(new SingleVisitingLogForElderSpec(visitingLogId.Value), context.CancellationToken)
            ?? ThrowVisitingLogNotFound();
        var visitingLogTitle = visitingLogData.Title;
        var semesterId = visitingLogData.SemesterId;

        result.Membership.AddRange(memberships.Select(m => new GroupCompositionMembership
        {
            StudentId = m.NsiStudentId.ToString(),
            Firstname = m.Firstname,
            Lastname = m.Lastname,
            Middlename = m.Middlename,
            MembershipType = m.MembershipType.ToGrpc(),
            MembershipRole = m.MembershipRole.ToGrpc()
        }));

        await _backgroundJobScheduler.FillVisitingLogCompositionFromAcademicGroup(visitingLogTitle, semesterId);

        return result;
    }

    public override async Task<AvailableDisciplinesResponse> GetAvailableDisciplines(GetAvailableDisciplinesRequest request, ServerCallContext context)
    {
        var visitingLogId = new VisitingLogId(request.VisitingLogId);

        await _authorizationService.RpcAuthorizeToVisitingLogRead(context, visitingLogId);

        var result = new AvailableDisciplinesResponse();
        var availableDisciplines = await _disciplineRep.ListAsync(new DisciplinesForElderSpec(visitingLogId.Value), context.CancellationToken);
        result.Discipline.AddRange(availableDisciplines.Select(
            d => new AvailableDiscipline
            {
                Id = d.Id.ToString(),
                Title = d.Title,
            }
        ));
        return result;
    }

    public override async Task<SingleVisitingLogResponse> GetSingleVisitingLog(GetSingleVisitingLogRequest request, ServerCallContext context)
    {
        var visitingLogId = new VisitingLogId(request.VisitingLogId);

        var isAdmin = (await _authorizationService.AuthorizeAsync(context.GetUser(), "Admin")).Succeeded;

        StudentMembership currentStudent = new StudentMembership() { 
             MembershipRole = AttendDatabase.Models.StudentMembershipRole.Unknown,
             MembershipType = AttendDatabase.Models.StudentMembershipType.Unknown
        };

        if (!isAdmin)
        {
            await _authorizationService.RpcAuthorizeToVisitingLogRead(context, visitingLogId);
            currentStudent = await _studentMembershipRep.SingleOrDefaultAsync(new SingleStudentInVisitingLog(context.GetHumanId(), visitingLogId.Value), context.CancellationToken)
                ?? throw new Exception("Student is not from current visiting log");
        }
        var membershipRole = isAdmin ? AttendDatabase.Models.StudentMembershipRole.Elder.ToGrpc() : currentStudent.MembershipRole.ToGrpc();
        var result = new SingleVisitingLogResponse();
        var visitingLog = await _visitingLogRep.SingleOrDefaultAsync(new SingleVisitingLogForElderSpec(visitingLogId.Value), context.CancellationToken)
            ?? ThrowVisitingLogNotFound();
        result.VisitingLogId = request.VisitingLogId;
        result.Title = visitingLog.Title;
        result.IsArchived = visitingLog.IsArchived;
        result.SemesterId = visitingLog.SemesterId.ToString();
        result.MembershipRole = membershipRole;
        return result;
    }

    public override async Task<AvailableLessonsResponse> GetAvailableLessons(GetAvailableLessonsRequest request, ServerCallContext context)
    {
        var visitingLogId = new VisitingLogId(request.VisitingLogId);
        await _authorizationService.RpcAuthorizeToVisitingLogRead(context, visitingLogId);

        var result = new AvailableLessonsResponse();

        var availableLessons = await _lessonRep.ListAsync(new GetLessonsForStudentDaySpec(visitingLogId.Value, request.Date.ToDateOnly()), context.CancellationToken);

        foreach (var lesson in availableLessons)
        {
            result.Lessons.Add(GrpcMapper.Map(lesson));
        }
        return result;
    }

    public override async Task<AttendanceForLessonResponse> GetAttendanceForLesson(GetAttendanceForLessonRequest request, ServerCallContext context)
    {
        var visitingLogId = new VisitingLogId(request.VisitingLogId);
        await _authorizationService.RpcAuthorizeToVisitingLogRead(context, visitingLogId);
        var lessonGuid = request.LessonId.AsGuid();

        var attendaces = await _attendanceService.GetAttendanceForLesson(visitingLogId.Value, lessonGuid, context.CancellationToken);

        var response = new AttendanceForLessonResponse();

        foreach (var attendance in attendaces)
        {
            response.Students.Add(attendance);
        }
        return response;
    }

    public override async Task<UpdateAttendanceResponse> UpdateAttendance(UpdateAttendanceRequest request, ServerCallContext context)
    {
        // Вынести в авторизацию как canUpdateAttendance
        var isTeacher = (await _authorizationService.AuthorizeAsync(context.GetUser(), "Teacher")).Succeeded;
        var updateAttendanceResult = await _attendanceService.UpdateAttendance(request, context.GetHumanId(), isTeacher, context.CancellationToken);
        return updateAttendanceResult.Match(
            error => throw new RpcException(new Status(StatusCode.NotFound, $"lesson {error.LessonId} not found")),
            result => result);
    }

    public override async Task<GetDataForLessonCreationResponse> GetDataForLessonCreation(GetDataForLessonCreationRequest request, ServerCallContext context)
    {
        var visitingLogId = new VisitingLogId(request.VisitingLogId);
        await _authorizationService.RpcAuthorizeToVisitingLogRead(context, visitingLogId);

        var result = new GetDataForLessonCreationResponse();

        var timeSlots = await _timeSlotRep.ListAsync(new TimeSlotDataForElderSpec(visitingLogId.Value), context.CancellationToken);

        var lessonDate = request.LessonDate.ToDateTimeOffset().Date;
        lessonDate = DateTime.SpecifyKind(lessonDate, DateTimeKind.Utc);

        foreach (var timeslot in timeSlots)
        {
            var lessonStart = new DateTimeOffset(lessonDate)
                .AddMinutes(timeslot.StartMinutesUTC);
            var chosenLesson = await _lessonRep.ListAsync(new GetLessonThroughTimeSlotForElderSpec(lessonStart, visitingLogId.Value));
            if (chosenLesson.Count == 0)
            {
                result.TimeSlots.Add(
                    new Timeslot
                    {
                        TimeslotId = timeslot.Id.ToString(),
                        StartMinutesUTC = timeslot.StartMinutesUTC,
                        EndMinutesUTC = timeslot.EndMinutesUTC
                    });
            }
        }

        var availableDisciplines = await _disciplineRep.ListAsync(new DisciplinesForElderSpec(visitingLogId.Value), context.CancellationToken);
        result.Disciplines.AddRange(availableDisciplines.Select(
            d => new AvailableDiscipline
            {
                Id = d.Id.ToString(),
                Title = d.Title,
            }
        ));
        var lessonTypes = await _lessonTypeRep.ListAsync(new LessonTypeDataForElderSpec(), context.CancellationToken);
        result.LessonTypes.AddRange(lessonTypes.Select(
            lt => new Lessontype
            {
                Id = lt.Id.ToString(),
                LessonType = lt.LessonTypeName
            }
        ));
        return result;
    }

    public override async Task<CreateLessonResponse> CreateLesson(CreateLessonRequest request, ServerCallContext context)
    {
        var visitingLogId = new VisitingLogId(request.VisitingLogId);
        await _authorizationService.RpcAuthorizeToVisitingLogRead(context, visitingLogId);

        var hasAccessToLog = await _studentMembershipRep.AnyAsync(new StudentIsElderForVisitingLog(context.GetHumanId(), visitingLogId.Value), context.CancellationToken);

        if (!hasAccessToLog)
        {
            _logger.LogInformation("Elder trying to create a lesson is not elder of current group");
            return new CreateLessonResponse
            {
                Result = CreateLessonResult.Error
            };
        }

        var timeSlotGuid = request.TimeSlotId.AsGuid();
        var disciplineGuid = request.DisciplineId.AsGuid();
        var lessonTypeGuid = request.LessonTypeId.AsGuid();

        var lessonDate = request.LessonDate.ToDateTimeOffset().Date;
        lessonDate = DateTime.SpecifyKind(lessonDate, DateTimeKind.Utc);

        var chosenTimeSlot = await _timeSlotRep.SingleOrDefaultAsync(new GetTimeSlotForElderSpec(timeSlotGuid), context.CancellationToken);

        if (chosenTimeSlot is null)
        {
            return new CreateLessonResponse
            {
                Result = CreateLessonResult.Error
            };
        }

        var lessonStart = new DateTimeOffset(lessonDate)
            .AddMinutes(chosenTimeSlot.StartMinutesUTC);
        var lessonEnd = lessonDate
            .AddMinutes(chosenTimeSlot.EndMinutesUTC);

        var chosenLesson = await _lessonRep.AnyAsync(new GetLessonThroughTimeSlotForElderSpec(lessonStart, visitingLogId.Value));

        if (chosenLesson)
        {
            _logger.LogInformation("Elder trying to create a lesson on already existing lesson time {LessonStart}", lessonStart);
            return new CreateLessonResponse
            {
                Result = CreateLessonResult.Error
            };
        }

        var newLesson = new Lesson
        {
            DisciplineId = disciplineGuid,
            LessonTypeId = lessonTypeGuid,
            Start = lessonStart,
            End = lessonEnd
        };
        var lessonToLogLink = new VisitingLogLesson
        {
            Lesson = newLesson,
            VisitingLogId = visitingLogId.Value
        };

        _attendDbContext.Lessons.Add(newLesson);
        _attendDbContext.VisitingLogLessons.Add(lessonToLogLink);

        try
        {
            await _attendDbContext.SaveChangesAsync();
            _logger.LogInformation("Created a new lesson with id {NewLessonId}", newLesson.Id);
            return new CreateLessonResponse
            {
                Result = CreateLessonResult.Ok
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't create a new lesson");
            return new CreateLessonResponse
            {
                Result = CreateLessonResult.Error
            };
        }
    }

    private static VisitingLog ThrowVisitingLogNotFound()
    {
        throw new RpcException(new Status(StatusCode.NotFound, "Visiting log not found"));
    }
}
