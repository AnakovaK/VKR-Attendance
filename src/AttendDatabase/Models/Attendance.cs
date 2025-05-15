namespace RTUAttendAPI.AttendDatabase.Models;
public class Attendance
{
    public Guid Id { get; set; }
    public required Guid StudentId { get; set; }
    public NsiStudent? Student { get; set; }

    // TODO: продумать, нужно ли подтверждение каждого участия в частности
    public Guid? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public required Guid LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public required AttendType Attend { get; set; }
    public required AuthorType Author { get; set; }
    public List<AttendanceEvent> Events { get; set; } = new List<AttendanceEvent>();
}

public enum AuthorType
{
    /// <summary>
    /// Неизвестный автор
    /// </summary>
    Unknown,
    /// <summary>
    /// Староста
    /// </summary>
    Elder,
    /// <summary>
    /// Рядовой студент
    /// </summary>
    Student,
    /// <summary>
    /// Преподаватель
    /// </summary>
    Teacher
}

public enum AttendType
{
    /// <summary>
    /// Неизвестный тип
    /// </summary>
    Unknown,
    /// <summary>
    /// Отсутствует
    /// </summary>
    Absent,
    /// <summary>
    /// Отсутствует по уважительной причине
    /// </summary>
    ExcusedAbsence,
    /// <summary>
    /// Присутствует
    /// </summary>
    Present
}
public static class AttendTypeLogic
{
    public static readonly IReadOnlyList<AttendType> AbsentTypes = new AttendType[]
    {
        AttendType.Absent,
        AttendType.ExcusedAbsence,
    };
}
