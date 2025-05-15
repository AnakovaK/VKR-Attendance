namespace RTUAttendAPI.AttendDatabase.Models;
public class Discipline
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public ICollection<DisciplineVisitingLog> VisitingLogs { get; set; } = new List<DisciplineVisitingLog>();
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
