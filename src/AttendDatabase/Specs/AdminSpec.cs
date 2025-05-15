using Ardalis.Specification;

using RTUAttendAPI.AttendDatabase.Models;

namespace RTUAttendAPI.AttendDatabase.Specs;
public class GetStudentByMembershipIdSpec : SingleResultSpecification<StudentMembership>
{
    public GetStudentByMembershipIdSpec(Guid studentMembership)
    {
        Query
            .Where(sm => sm.Id == studentMembership)
            .AsNoTracking();
    }
}

public class GetStudentMembershipByIdsSpec : SingleResultSpecification<StudentMembership>
{
    public GetStudentMembershipByIdsSpec(Guid visitingLogId, Guid studentId)
    {
        Query
            .Where(sm => sm.NsiStudentId == studentId)
            .Where(sm =>sm.VisitingLogId == visitingLogId) 
            .AsNoTracking(); 
    }
}
