using System;
using System.Linq.Expressions;
using System.Net.Http.Headers;

using Ardalis.Specification;

using RTUAttendAPI.AttendDatabase.Models;

namespace RTUAttendAPI.AttendDatabase.Specs;

public record AvailableVisitingLog(
    Guid VisitingLogId,
    string Title,
    bool IsActive,
    int StudentsCount
);
public class AvailableVisitingLogsForStudentSpec : Specification<VisitingLog, AvailableVisitingLog>
{
    public AvailableVisitingLogsForStudentSpec(Guid humanId)
    {
        Query
            .Select(i => new AvailableVisitingLog(
                i.Id,
                i.Title,
                !i.IsArchived,
                i.StudentMemberships.Count(m => m.MembershipType == StudentMembershipType.Active)
            ))
            .Where(log => log.StudentMemberships.Any(m => m.NsiStudent!.NsiHumanId == humanId && m.MembershipType == StudentMembershipType.Active))
            .AsNoTracking();
    }
}

public record GroupComposition(
    Guid NsiStudentId,
    string Firstname,
    string Lastname,
    string? Middlename,
    StudentMembershipType MembershipType,
    StudentMembershipRole MembershipRole
);

public class GroupCompositionForElderSpec : Specification<StudentMembership, GroupComposition>
{
    public GroupCompositionForElderSpec(Guid visitingLogId)
    {
        Query
            .Select(sm => new GroupComposition(
                sm.NsiStudentId,
                sm.NsiStudent!.NsiHuman!.Firstname,
                sm.NsiStudent.NsiHuman.Lastname,
                sm.NsiStudent.NsiHuman.Middlename,
                sm.MembershipType,
                sm.MembershipRole
             ))
            .OrderBy(sm => sm.NsiStudent!.NsiHuman!.Lastname)
                .ThenBy(sm => sm.NsiStudent!.NsiHuman!.Firstname)
                    .ThenBy(sm => sm.NsiStudent!.NsiHuman!.Middlename)
                        .ThenBy(sm => sm.NsiStudent!.PersonalNumber)
            .Where(sm => sm.VisitingLogId == visitingLogId)
            .AsNoTracking();
    }
}

public class DisciplinesForElderSpec : Specification<Discipline>
{
    public DisciplinesForElderSpec(Guid visitingLogId)
    {
        Query
            .Where(d => d.VisitingLogs.Any(vl => vl.FkVisitingLogId == visitingLogId))
            .AsNoTracking();
    }
}

public class SingleVisitingLogForElderSpec : SingleResultSpecification<VisitingLog>
{
    public SingleVisitingLogForElderSpec(Guid visitingLogId)
    {
        Query
            .Where(j => j.Id == visitingLogId)
            .AsNoTracking();
    }
}

public class SingleStudentInVisitingLog : SingleResultSpecification<StudentMembership>
{
    public SingleStudentInVisitingLog(Guid humanId, Guid visitingLogId)
    {
        Query
            .Where(sm => sm.NsiStudent!.NsiHumanId == humanId)
            .Where(sm => sm.VisitingLogId == visitingLogId)
            .AsNoTracking();
    }
}

public class AttendancesForUpdate : Specification<Attendance>
{
    public AttendancesForUpdate(Guid lessonId, Guid[] studentIds)
    {
        Query
            .Where(a => a.LessonId == lessonId)
            .Where(a => studentIds.Contains(a.StudentId))
            .AsNoTracking();
    }
}

public class AttendanceForUpdate : Specification<Attendance>
{
    public AttendanceForUpdate(Guid lessonId, Guid studentId)
    {
        Query
            .Where(a => a.LessonId == lessonId)
            .Where(a => a.StudentId == studentId)
            .AsNoTracking();
    }
}

public record StudentsPresentCountForLesson(int StudentsPresent, int StudentsTotalCount);
public class AttendancesCountForLesson : Specification<Lesson, StudentsPresentCountForLesson>
{
    public AttendancesCountForLesson(Guid lessonId)
    {
        Query
            .Select(l => new StudentsPresentCountForLesson(
                l.Attendances.Count(a => a.Attend == AttendType.Present),
                l.VisitingLogs.Sum(l => l.VisitingLog!.StudentMemberships.Count)))
            .Where(l => l.Id == lessonId)
            .AsNoTracking();
    }
}

public class StudentIsElderForVisitingLog : SingleResultSpecification<StudentMembership>
{
    public StudentIsElderForVisitingLog(Guid humanId, Guid visitingLogId)
    {
        Query
            .Where(sm =>
                sm.NsiStudent!.NsiHumanId == humanId &&
                    sm.VisitingLogId == visitingLogId &&
                        sm.MembershipRole == StudentMembershipRole.Elder)
            .AsNoTracking();
    }
}

// TODO: вынести в authorization service, чтобы там проверять и на преподавателя
public class StudentCanUpdateAttendance : SingleResultSpecification<StudentMembership>
{
    public StudentCanUpdateAttendance(Guid studentHumanId, Guid lessonId)
    {
        Query
            .Where(sm =>
                sm.NsiStudent!.NsiHumanId == studentHumanId &&
                    sm.MembershipRole == StudentMembershipRole.Elder &&
                        sm.VisitingLog!.Lessons.Any(l => l.LessonId == lessonId))
            .AsNoTracking();
    }
}

public class IsStudentHasAccessToLessonSpec : SingleResultSpecification<StudentMembership>
{
    public IsStudentHasAccessToLessonSpec(Guid studentId, Guid lessonId)
    {
        Query
            .Where(s => s.NsiStudentId == studentId)
            .Where(s => s.VisitingLog!.Lessons.Any(l => l.LessonId == lessonId))
            .AsNoTracking();
    }
}

/// <summary>
/// Возвращает идентификатор студента, под который человек относится к занятию (если относится)
/// </summary>
public class StudentIdByHumanIdForLesson : SingleResultSpecification<StudentMembership, Guid>
{
    public StudentIdByHumanIdForLesson(Guid studentHumanId, Guid lessonId)
    {
        Query
            .Select(s => s.NsiStudentId)
            .Where(s => s.NsiStudent!.NsiHumanId == studentHumanId)
            .Where(s => s.VisitingLog!.Lessons.Any(l => l.LessonId == lessonId))
            .AsNoTracking();
    }
}

public class TimeSlotDataForElderSpec : Specification<TimeSlot>
{
    public TimeSlotDataForElderSpec(Guid visitingLogId)
    {
        Query
            .Where(t => t.Semester!.VisitingLogs.Any(vl => vl.Id == visitingLogId))
            .AsNoTracking();
    }
}

public class LessonTypeDataForElderSpec : Specification<LessonType>
{
    public LessonTypeDataForElderSpec()
    {
        Query
            .AsNoTracking();
    }
}

public class GetTimeSlotForElderSpec : SingleResultSpecification<TimeSlot>
{
    public GetTimeSlotForElderSpec(Guid timeSlotId)
    {
        Query
            .Where(ts => ts.Id == timeSlotId)
            .AsNoTracking();
    }
}
public class GetLessonThroughTimeSlotForElderSpec : Specification<Lesson>
{
    public GetLessonThroughTimeSlotForElderSpec(DateTimeOffset lessonStart, Guid visitingLogGuid)
    {
        Query
            .Where(l => l.Start == lessonStart)
            .Where(l => l.VisitingLogs.Any(vl => vl.VisitingLogId == visitingLogGuid))
            .AsNoTracking();
    }
}
