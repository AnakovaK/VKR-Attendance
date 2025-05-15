using System.Linq.Expressions;

using Ardalis.Specification;

using Microsoft.EntityFrameworkCore;

using RTUAttendAPI.AttendDatabase.Models;

namespace RTUAttendAPI.AttendDatabase.Specs;

public class FindLessonBasedOnSchedule : Specification<Lesson, LessonDbView>
{
    public FindLessonBasedOnSchedule(DateTimeOffset lessonStart, Guid eventId)
    {
        Query
            .Select(LessonToView.Expression)
            .Where(l => l.Start == lessonStart)
            .Where(l => l.CreatedFromScheduleEventId == eventId)
            .AsNoTracking();
    }
}


public class SearchLessonsByMatchSpec : Specification<Lesson, LessonDbView>
{
    public SearchLessonsByMatchSpec(string match, DateOnly day)
    {
        // TODO: Получается, что ищем не по московскому дню. Фактически это вряд-ли будет проблемой, но стоит подумать
        var dayStart = new DateTimeOffset(day.Year, day.Month, day.Day, 0, 0, 0, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);
        var ilikeRow = $"{match}%";
        Query
            .Select(LessonToView.Expression)
            .Where(l => l.Start >= dayStart && l.End <= dayEnd)
            .Where(l =>
                EF.Functions.ILike(l.Discipline!.Title, ilikeRow) ||
                l.VisitingLogs.Any(vl => EF.Functions.ILike(vl.VisitingLog!.Title, ilikeRow)) ||
                l.Auditoriums.Any(a => EF.Functions.ILike(a.Auditorium!.Number, ilikeRow))
            )
            .OrderBy(l => l.Start)
                .ThenBy(l => l.Id)
            .Take(20)
            .AsNoTracking();
    }
}

public class GetLessonsForStudentDaySpec : Specification<Lesson, LessonDbView>
{
    public GetLessonsForStudentDaySpec(Guid visitingLogId, DateOnly day)
    {
        // TODO: Получается, что ищем не по московскому дню. Фактически это вряд-ли будет проблемой, но стоит подумать
        var dayStart = new DateTimeOffset(day.Year, day.Month, day.Day, 0, 0, 0, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);
        Query
            .Select(LessonToView.ExpressionByVisitingLog(visitingLogId))
            .Where(l => l.Start >= dayStart && l.End <= dayEnd)
            .Where(l => l.VisitingLogs.Any(vl => vl.VisitingLogId == visitingLogId))
            .OrderBy(l => l.Start)
                .ThenBy(l => l.Id)
            .AsNoTracking();
    }
}

public class GetLessonsForTeacherDaySpec : Specification<Lesson, LessonDbView>
{
    public GetLessonsForTeacherDaySpec(Guid teacherHumanId, DateOnly day)
    {
        // TODO: Получается, что ищем не по московскому дню. Фактически это вряд-ли будет проблемой, но стоит подумать
        var dayStart = new DateTimeOffset(day.Year, day.Month, day.Day, 0, 0, 0, TimeSpan.Zero);
        var dayEnd = dayStart.AddDays(1);
        Query
            .Select(LessonToView.Expression)
            .Where(l => l.Start >= dayStart && l.End <= dayEnd)
            .Where(l => l.Teachers.Any(lt => lt.Teacher!.NsiHumanId == teacherHumanId))
            .OrderBy(l => l.Start)
                .ThenBy(l => l.Id)
            .AsNoTracking();
    }
}

public class GetLessonsByIdSpec : SingleResultSpecification<Lesson, LessonDbView>
{
    public GetLessonsByIdSpec(Guid lessonId)
    {
        Query
            .Select(LessonToView.Expression)
            .Where(l => l.Id == lessonId)
            .AsNoTracking();
    }
}

public static class LessonToView
{
    public static Expression<Func<Lesson, LessonDbView>> ExpressionByVisitingLog(Guid visitingLogId) =>
        l => new LessonDbView(
                l.Id,
                l.Discipline!.Title,
                l.LessonType!.LessonTypeName,
                l.Start,
                l.End,
                l.IsValidated,
                l.Attendances.Any(),
                l.Attendances.Where(a => a.Student!.StudentMemberships.Any(sm => sm.VisitingLogId == visitingLogId)).Count(a => AttendTypeLogic.AbsentTypes.Contains(a.Attend)),
                l.Auditoriums.Select(a => new AuditoriumView(a.Auditorium!.Id, a.Auditorium.Number, a.Auditorium.Campus)).ToArray(),
                l.Teachers.Select(t =>
                    new HumanView(
                        t.Teacher!.NsiHumanId,
                        t.Teacher.NsiHuman!.Lastname,
                        t.Teacher.NsiHuman!.Firstname,
                        t.Teacher.NsiHuman!.Middlename))
                .ToArray(),
                l.VisitingLogs.Select(vl => new VisitingLogView(
                    vl.VisitingLogId,
                    vl.VisitingLog!.Title,
                    vl.VisitingLog.IsArchived,
                    vl.VisitingLog.SemesterId
                ))
                .ToArray()
            );
    public static Expression<Func<Lesson, LessonDbView>> Expression { get; } =
        l => new LessonDbView(
                l.Id,
                l.Discipline!.Title,
                l.LessonType!.LessonTypeName,
                l.Start,
                l.End,
                l.IsValidated,
                l.Attendances.Any(),
                l.Attendances.Count(a => AttendTypeLogic.AbsentTypes.Contains(a.Attend)),
                l.Auditoriums.Select(a => new AuditoriumView(a.Auditorium!.Id, a.Auditorium.Number, a.Auditorium.Campus)).ToArray(),
                l.Teachers.Select(t =>
                    new HumanView(
                        t.Teacher!.NsiHumanId,
                        t.Teacher.NsiHuman!.Lastname,
                        t.Teacher.NsiHuman!.Firstname,
                        t.Teacher.NsiHuman!.Middlename))
                .ToArray(),
                l.VisitingLogs.Select(vl => new VisitingLogView(
                    vl.VisitingLogId,
                    vl.VisitingLog!.Title,
                    vl.VisitingLog.IsArchived,
                    vl.VisitingLog.SemesterId
                ))
                .ToArray()
            );
}

public class AttendanceForLessonSpec : Specification<Attendance>
{
    public AttendanceForLessonSpec(Guid lessonId)
    {
        Query
            .Where(a => a.LessonId == lessonId)
            .OrderBy(a => a.Student!.NsiHuman!.Lastname)
                .ThenBy(a => a.Student!.NsiHuman!.Firstname)
                    .ThenBy(a => a.Student!.NsiHuman!.Middlename)
                        .ThenBy(a => a.Student!.PersonalNumber)
            .AsNoTracking();
    }
}

public record LessonDbView(
    Guid Id,
    string Discipline,
    string LessonType,
    DateTimeOffset Start,
    DateTimeOffset End,
    bool IsValidated,
    bool AttendancesExists,
    int AbsenteesCount,
    AuditoriumView[] Auditoriums,
    HumanView[] Teachers,
    VisitingLogView[] VisitingLogs
);

public record AuditoriumView(
    Guid Id,
    string Number,
    string Building
);

public record HumanView(
    Guid Id,
    string Lastname,
    string Firstname,
    string? Middlename
);

public record VisitingLogView(
    Guid Id,
    string Title,
    bool IsArchieved,
    Guid SemesterId
);
