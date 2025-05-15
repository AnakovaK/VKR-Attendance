using Ardalis.Specification.EntityFrameworkCore;

using AttendDatabase;

namespace RTUAttendAPI.AttendDatabase.Repositories;
public class AttendRepository<T> : RepositoryBase<T> where T : class
{
    public AttendRepository(AttendDbContext dbContext) : base(dbContext)
    {
    }
}
