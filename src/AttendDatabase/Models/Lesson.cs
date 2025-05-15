namespace RTUAttendAPI.AttendDatabase.Models;
public class Lesson
{
    public Guid Id { get; set; }

    public Guid DisciplineId { get; set; }
    public Discipline? Discipline { get; set; }

    public Guid? LessonTypeId { get; set; }
    public LessonType? LessonType { get; set; }

    public required DateTimeOffset Start { get; set; }
    public required DateTimeOffset End { get; set; }

    /// <summary>
    /// Подтверждено преподавателем. Если подтверждено - студенты отмечаться не могут (в том числе от имени старосты)
    /// </summary>
    public bool IsValidated { get; set; } = default!;
    /// <summary>
    /// Идентификатор события из ICal, на основании которого был создан <see cref="Lesson"/>
    /// </summary>
    public Guid? CreatedFromScheduleEventId { get; set; }

    public ICollection<TeacherLesson> Teachers { get; set; } = new List<TeacherLesson>();
    public ICollection<AuditoriumLesson> Auditoriums { get; set; } = new List<AuditoriumLesson>();
    public ICollection<VisitingLogLesson> VisitingLogs { get; set; } = new List<VisitingLogLesson>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
