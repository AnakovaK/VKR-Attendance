namespace RTUAttendAPI.AttendDatabase.Models;
public class Teacher
{
    public Guid Id { get; set; }
    public Guid NsiHumanId { get; set; }
    public NsiHuman? NsiHuman { get; set; }
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<TeacherLesson> Lessons { get; set; } = new List<TeacherLesson>();
}
