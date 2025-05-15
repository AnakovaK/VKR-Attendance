namespace RTUAttendAPI.AttendDatabase.Models;
public class AuditoriumLesson
{
    public Guid Id { get; set; }
    
    public Guid AuditoriumId { get; set; }
    public Auditorium? Auditorium { get; set; }

    public Guid LessonId { get; set; }
    public Lesson? Lesson { get; set; }

}
