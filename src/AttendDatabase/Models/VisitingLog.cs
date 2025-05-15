namespace RTUAttendAPI.AttendDatabase.Models;
public class VisitingLog
{
    public Guid Id { get; set; }
    public required string Title { get; set; } = default!;

    public required bool IsArchived { get; set; }

    public Guid SemesterId { get; set; }
    public Semester? Semester { get; set; }
    public VisitingLogSource? Source { get; set; }

    public ICollection<StudentMembership> StudentMemberships { get; set; } = new List<StudentMembership>();
    public ICollection<DisciplineVisitingLog> Disciplines { get; set; } = new List<DisciplineVisitingLog>();
    public ICollection<VisitingLogLesson> Lessons { get; set; } = new List<VisitingLogLesson>();
}

public enum VisitingLogSource
{
    /// <summary>
    /// Неизвестный источник
    /// </summary>
    Unknown,
    /// <summary>
    /// Полученный из тандема
    /// </summary>
    Schedule,
    /// <summary>
    /// Созданный вручную
    /// </summary>
    Manual
}
