namespace RTUAttendAPI.AttendDatabase.Models;

public class Semester
{
    public Guid Id { get; set; }
    public required string Title { get; set; } = default!;
    public required DateTimeOffset Start { get; set; }
    public required DateTimeOffset End { get; set; }

    public ICollection<VisitingLog> VisitingLogs { get; set; } = new List<VisitingLog>();
    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}
