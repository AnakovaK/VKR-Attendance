namespace RTUAttendAPI.AttendDatabase.Models;
public class LessonType
{
    public Guid Id { get; set; }
    public required string LessonTypeName { get; set; }
    
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
