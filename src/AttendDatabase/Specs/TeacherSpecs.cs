using Ardalis.Specification;

using RTUAttendAPI.AttendDatabase.Models;

namespace RTUAttendAPI.AttendDatabase.Specs;
public class GetAllSemesetersForTeacherSpec : Specification<Semester>
{
    public GetAllSemesetersForTeacherSpec()
    {
        Query
            .AsNoTracking();
    }
}

public record VisitingLogsForTeacher(
    Guid Id,
    string Title,
    bool IsArchieved,
    int StudentsCount
);

public class GetVisitingLogsOfSemesterForTeacherSpec : Specification<VisitingLog, VisitingLogsForTeacher>
{
    public GetVisitingLogsOfSemesterForTeacherSpec(Guid semesterId)
    {
        Query
            .Select(vl => new VisitingLogsForTeacher(
                vl.Id,
                vl.Title,
                vl.IsArchived,
                vl.StudentMemberships.Count
            ))
            .Where(vl => vl.SemesterId == semesterId)
            .OrderBy(vl => vl.Title)
                .ThenBy(vl => vl.Id)
            .AsNoTracking();
    }
}


public record VisitingLogInfo(Guid Id, string Title);
public class GetVisitingLogsForLesson : Specification<Lesson, VisitingLogInfo>
{
    public GetVisitingLogsForLesson(Guid lessonId)
    {
        Query
             .SelectMany(l => l.VisitingLogs
                    .Select(l => l.VisitingLog!)
                    .OrderBy(vl => vl.Title)
                    .Select(vl => new VisitingLogInfo(vl.Id, vl.Title))
            )
            .Where(l => l.Id == lessonId)
            .OrderBy(l => l.Start)
                .ThenBy(l => l.Id)
            .AsNoTracking();
    }
}


public class GetVisitingLogByIdSpec : SingleResultSpecification<VisitingLog>
{
    public GetVisitingLogByIdSpec(Guid visitingLogId)
    {
        Query
            .Where(vl => vl.Id == visitingLogId)
            .AsNoTracking();
    }
}

public record GroupCompositionForTeacher(
    Guid NsiStudentId,
    string Firstname,
    string Lastname,
    string? Middlename,
    StudentMembershipType MembershipType,
    StudentMembershipRole MembershipRole
);

public class GetStudentMembershipsByVisitingLogIdSpec : Specification<StudentMembership, GroupCompositionForTeacher>
{
    public GetStudentMembershipsByVisitingLogIdSpec(Guid visitingLogId)
    {
        Query
            .Select(sm => new GroupCompositionForTeacher(
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

public class GetTeacherIdByHumanId : SingleResultSpecification<Teacher, Guid>
{
    public GetTeacherIdByHumanId(Guid humanId)
    {
        Query
            .Select(t => t.Id)
            .Where(t => t.NsiHumanId == humanId)
            .AsNoTracking();
    }
}

public class GetTeacherLessonLinkByIds : SingleResultSpecification<TeacherLesson>
{
    public GetTeacherLessonLinkByIds(Guid teacherId, Guid lessonId)
    {
        Query
            .Where(tl => tl.TeacherId == teacherId)
            .Where(tl => tl.LessonId == lessonId)
            .AsNoTracking();
    }
}

