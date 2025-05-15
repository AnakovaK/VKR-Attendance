namespace RTUAttendAPI.AttendDatabase.Models;
public class TeacherLesson
{
    public Guid Id { get; set; }

    public Guid TeacherId { get; set; }
    public Teacher? Teacher { get; set; }

    public Guid LessonId { get; set; }
    public Lesson? Lesson { get; set; }
}
