namespace RTUAttendAPI.AttendDatabase.Models;
public class AttendanceEvent
{
    public Guid Id { get; set; }
    public Guid AttendanceId { get; set; }
    public Attendance? Attendance { get; set; }
    public Guid AuthorHumanId { get; set; }
    public NsiHuman? AuthorHuman { get; set; }
    public required DateTimeOffset Date { get; set; }
    public required AttendType AttendType { get; set; }
    public required AuthorType Author { get; set; }
}
