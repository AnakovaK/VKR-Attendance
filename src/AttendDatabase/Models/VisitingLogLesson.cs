namespace RTUAttendAPI.AttendDatabase.Models;
public class VisitingLogLesson
{
    public Guid Id { get; set; }

    public Guid VisitingLogId { get; set; }
    public VisitingLog? VisitingLog { get; set; }

    public Guid LessonId { get; set; }
    public Lesson? Lesson { get; set; }
}
