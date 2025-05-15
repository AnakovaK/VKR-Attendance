using Ardalis.Specification;

using AttendDatabase;

using RTUAttendAPI.AttendDatabase.Models;

namespace RTUAttendAPI.AttendDatabase.Specs;

public class GroupCompositionForTandemSpec : SingleResultSpecification<VisitingLog>
{
    public GroupCompositionForTandemSpec(string groupTitle, Guid semesterId)
    {
        Query
            .Include(vl => vl.StudentMemberships)
            .ThenInclude(sm => sm.NsiStudent)
            .Where(vl => vl.Title == groupTitle && vl.SemesterId == semesterId)
            .AsNoTracking();
    }
}

public class GetNsiHumanForTandemSpec : Specification<NsiHuman>
{
    public GetNsiHumanForTandemSpec(Guid nsiHumanId)
    {
        Query
            .Where(h => h.Id == nsiHumanId)
            .AsNoTracking();
    }
}

public class GetNsiStudentForTandemSpec : Specification<NsiStudent>
{
    public GetNsiStudentForTandemSpec(Guid nsiStudentId)
    {
        Query
            .Where(s => s.Id == nsiStudentId)
            .AsNoTracking();
    }
}

public record HumanLoginInfo(Guid HumanId, bool IsTeacher);
public class GetHumanLoginInfoByLoginIdSpec : Specification<NsiHuman, HumanLoginInfo>
{
    public GetHumanLoginInfoByLoginIdSpec(Guid loginId)
    {
        Query
            .Select(p => new HumanLoginInfo(p.Id, p.Teachers.Any()))
            .Where(p => p.Id == loginId
                || p.NsiStudents.Any(s => s.Id == loginId)
                || p.Logins.Any(l => l.LoginId == loginId))
            .AsNoTracking();
    }
}
