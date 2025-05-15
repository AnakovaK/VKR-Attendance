using System.Threading;

using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

using AttendDatabase;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using OneOf;

using RTUAttendAPI.API.Authorization.Entities;
using RTUAttendAPI.API.Extensions;
using RTUAttendAPI.AttendDatabase.Models;
using RTUAttendAPI.AttendDatabase.Specs;

using RtuTc.RtuAttend.App;
using RtuTc.RtuAttend.Models;

namespace RTUAttendAPI.API.Services.BusinessLogic;

public class AttendanceService
{
    private readonly IRepositoryBase<StudentMembership> _studentMembershipRep;
    private readonly IRepositoryBase<Lesson> _lessonRep;
    private readonly IRepositoryBase<Attendance> _attendanceRep;
    private readonly AttendDbContext _attendDbContext;
    private readonly ILogger<AttendanceService> _logger;
    public AttendanceService(
        IRepositoryBase<StudentMembership> studentMembershipRep,
        IRepositoryBase<Lesson> lessonRep,
        IRepositoryBase<Attendance> attendanceRep,
        AttendDbContext attendDbContext, // используется для обновления TODO: разнести выборки и изменения в разные grpc сервисы (мини CQRS)
        ILogger<AttendanceService> logger
    )
    {
        _studentMembershipRep = studentMembershipRep;
        _lessonRep = lessonRep;
        _attendanceRep = attendanceRep;
        _attendDbContext = attendDbContext;
        _logger = logger;
    }

    internal async Task<OneOf<LessonNotFound, UpdateAttendanceResponse>> UpdateAttendance(
        UpdateAttendanceRequest request,
        Guid humanId,
        bool isTeacher, // Вынести проверку в отдельный шаг в авторизационном сервисе. Этот сервис должен просто делать
        CancellationToken cancellationToken)
    {
        var response = new UpdateAttendanceResponse();

        var lessonGuid = request.LessonId.AsGuid();

        var currentLesson = await _lessonRep.GetByIdAsync(lessonGuid, cancellationToken);
        if (currentLesson is null)
        {
            return new LessonNotFound(lessonGuid);
        }
        var canUpdateAttendance = isTeacher || await _studentMembershipRep.AnyAsync(new StudentCanUpdateAttendance(humanId, lessonGuid), cancellationToken);

        if (!canUpdateAttendance)
        {
            _logger.LogInformation("User {HumanId} trying update lesson {LessonId} but doesn't have access", humanId, lessonGuid);
            return new UpdateAttendanceResponse
            {
                Result = UpdateAttendanceResult.Error
            };
        }

        if (currentLesson.IsValidated && !isTeacher)
        {
            _logger.LogInformation("Elder trying to update attendances on an already approved by teacher lesson");
            return new UpdateAttendanceResponse
            {
                Result = UpdateAttendanceResult.Error
            };
        }

        var changeRecords = request.Records
            .DistinctBy(r => r.StudentId)
            .Select(r => new
            {
                StudentId = r.StudentId.AsGuid(),
                r.AttendType
            })
            .ToArray();

        foreach (var changeRecord in changeRecords)
        {
            var hasAccessToLesson = await _studentMembershipRep.AnyAsync(new IsStudentHasAccessToLessonSpec(changeRecord.StudentId, currentLesson.Id), cancellationToken);
            if (!hasAccessToLesson)
            {
                _logger.LogInformation($"Trying to update attendance of student which is not from current visiting log");
                return new UpdateAttendanceResponse
                {
                    Result = UpdateAttendanceResult.Error
                };
            }
        }

        var attendancesFromDb = await _attendanceRep.ListAsync(new AttendancesForUpdate(lessonGuid, changeRecords.Select(r => r.StudentId).ToArray()), cancellationToken);

        foreach (var changeRecord in changeRecords)
        {
            var fromDb = attendancesFromDb.SingleOrDefault(a => a.StudentId == changeRecord.StudentId);
            if (fromDb is null)
            {
                fromDb = new Attendance
                {
                    StudentId = changeRecord.StudentId,
                    LessonId = lessonGuid,
                    Attend = changeRecord.AttendType.FromGrpc(),
                    Author = isTeacher ? AuthorType.Teacher : AuthorType.Elder,
                };
                var updatedAttendanceEvent = new AttendanceEvent
                {
                    Attendance = fromDb,
                    Date = DateTimeOffset.UtcNow,
                    Author = isTeacher ? AuthorType.Teacher : AuthorType.Elder,
                    AuthorHumanId = humanId,
                    AttendType = changeRecord.AttendType.FromGrpc()
                };
                fromDb.Events.Add(updatedAttendanceEvent);
                _attendDbContext.AttendanceEvents.Add(updatedAttendanceEvent);
                _attendDbContext.Attendances.Add(fromDb);
            }
            else
            {
                fromDb.Attend = changeRecord.AttendType.FromGrpc();
                fromDb.Author = isTeacher ? AuthorType.Teacher : AuthorType.Elder;
                var updatedAttendanceEvent = new AttendanceEvent
                {
                    AttendanceId = fromDb.Id,
                    Date = DateTimeOffset.UtcNow,
                    Author = isTeacher ? AuthorType.Teacher : AuthorType.Elder,
                    AuthorHumanId = humanId,
                    AttendType = changeRecord.AttendType.FromGrpc()
                };
                fromDb.Events.Add(updatedAttendanceEvent);
                _attendDbContext.AttendanceEvents.Add(updatedAttendanceEvent);
                _attendDbContext.Attendances.Update(fromDb);
            }
        }
        try
        {
            await _attendDbContext.SaveChangesAsync(cancellationToken);
            await ReplaceAttendUnknownStatusWithAbsent(lessonGuid, isTeacher ? AuthorType.Teacher : AuthorType.Elder, humanId, cancellationToken);
            return new UpdateAttendanceResponse
            {
                Result = UpdateAttendanceResult.Ok
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Can't update attendances");
            return new UpdateAttendanceResponse
            {
                Result = UpdateAttendanceResult.Error
            };
        }
    }
    public async Task<IReadOnlyCollection<AttendanceForLesson>> GetAttendanceForLesson(Guid visitingLogId, Guid lessonId, CancellationToken cancellationToken)
    {
        var attendance = await _attendanceRep.ListAsync(new AttendanceForLessonSpec(lessonId), cancellationToken);
        var memberships = await _studentMembershipRep.ListAsync(new GroupCompositionForElderSpec(visitingLogId), cancellationToken);

        var list = new List<AttendanceForLesson>();
        foreach (var membership in memberships)
        {
            var attendForLesson = new AttendanceForLesson
            {
                StudentId = membership.NsiStudentId.ToString(),
                Firstname = membership.Firstname,
                Lastname = membership.Lastname,
                Middlename = membership.Middlename
            };
            var attend = attendance.SingleOrDefault(a => a.StudentId == membership.NsiStudentId);
            if (attend is null)
            {
                attendForLesson.NoAttendType = new Empty();
            }
            else
            {
                attendForLesson.ExistingAttendType = attend.Attend.ToGrpc();
            }
            list.Add(attendForLesson);
        }
        return list;
    }

    public async Task ReplaceAttendUnknownStatusWithAbsent(Guid lessonId, AuthorType author, Guid humanId, CancellationToken cancellationToken)
    {
        var lesson = await _attendDbContext.Lessons.WithSpecification(new GetLessonsByIdSpec(lessonId)).SingleOrDefaultAsync(cancellationToken);
        if (lesson is null)
        {
            _logger.LogWarning("Lesson was not found by id {LessonId}", lessonId);
            return;
        }
        List<GroupCompositionForTeacher> currentLogMemberships = new();
        foreach (var log in lesson.VisitingLogs)
        {
            var fullLog = await _attendDbContext.VisitingLogs.WithSpecification(new GetVisitingLogByIdSpec(log.Id)).SingleOrDefaultAsync(cancellationToken);
            if (fullLog is null)
            {
                _logger.LogWarning("Log was not found by title {LogTitle}", log.Title);
                return;
            }
            var studentMembershipsOfLog = await _attendDbContext.StudentMemberships.WithSpecification(new GetStudentMembershipsByVisitingLogIdSpec(fullLog.Id)).ToListAsync(cancellationToken);
            currentLogMemberships.AddRange(studentMembershipsOfLog);
        }
        var attendancesForLesson = await _attendDbContext.Attendances.WithSpecification(new AttendanceForLessonSpec(lessonId)).ToListAsync(cancellationToken);
        foreach (var membership in currentLogMemberships)
        {
            if (!attendancesForLesson.Any(a => a.StudentId == membership.NsiStudentId))
            {
                var absentStatus = new Attendance
                {
                    StudentId = membership.NsiStudentId,
                    LessonId = lessonId,
                    Author = author,
                    Attend = AttendDatabase.Models.AttendType.Absent
                };
                absentStatus.Events.Add(new AttendanceEvent
                {
                    Attendance = absentStatus,
                    Date = DateTimeOffset.UtcNow,
                    Author = author,
                    AuthorHumanId = humanId,
                    AttendType = AttendDatabase.Models.AttendType.Absent
                });
                _attendDbContext.Attendances.Add(absentStatus);
            }
        }
        try
        {
            await _attendDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't replace unknown attendances with absent");
        }
    }

    public async Task<StudentsPresentCountForLesson?> GetStudentAttendancesCountForLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        return await _attendDbContext.Lessons
            .WithSpecification(new AttendancesCountForLesson(lessonId))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public enum SetAttendanceToPresentForStudentError
    {
        Unknown,
        NoLinkToLesson,
    }
    /// <summary>
    /// Обновляет статус человека в занятии. Если обновлено успешно - возвращает модель занятия, или ошибку
    /// </summary>
    /// <param name="studentHumanId">id человека</param>
    /// <param name="lessonId">id занятия</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<OneOf<SetAttendanceToPresentForStudentError, LessonDbView>> SetAttendanceToPresentForStudent(Guid studentHumanId, Guid lessonId, CancellationToken cancellationToken)
    {
        var studentId = await _studentMembershipRep.SingleOrDefaultAsync(new StudentIdByHumanIdForLesson(studentHumanId, lessonId), cancellationToken);
        if (studentId == default)
        {
            return SetAttendanceToPresentForStudentError.NoLinkToLesson;
        }
        var fromDb = await _attendDbContext.Attendances
            .WithSpecification(new AttendanceForUpdate(lessonId, studentId))
            .SingleOrDefaultAsync(cancellationToken);

        if (fromDb is null)
        {
            fromDb = new Attendance
            {
                StudentId = studentId,
                LessonId = lessonId,
                Attend = AttendDatabase.Models.AttendType.Present,
                Author = AuthorType.Student,
            };
            fromDb.Events.Add(new AttendanceEvent
            {
                Attendance = fromDb,
                Date = DateTimeOffset.UtcNow,
                Author = AuthorType.Student,
                AuthorHumanId = studentHumanId,
                AttendType = AttendDatabase.Models.AttendType.Present
            });
            _attendDbContext.Attendances.Add(fromDb);
        }
        else
        {
            fromDb.Attend = AttendDatabase.Models.AttendType.Present;
            fromDb.Author = AuthorType.Student;
            var updatedAttendanceEvent = new AttendanceEvent
            {
                AttendanceId = fromDb.Id,
                Date = DateTimeOffset.UtcNow,
                Author = AuthorType.Student,
                AuthorHumanId = studentHumanId,
                AttendType = AttendDatabase.Models.AttendType.Present
            };
            _attendDbContext.AttendanceEvents.Add(updatedAttendanceEvent);
            _attendDbContext.Attendances.Update(fromDb);
        }
        await _attendDbContext.SaveChangesAsync(cancellationToken);
        return await _attendDbContext.Lessons
            .WithSpecification(new GetLessonsByIdSpec(lessonId))
            .SingleAsync(cancellationToken);
    }
}
