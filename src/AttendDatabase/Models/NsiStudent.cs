namespace RTUAttendAPI.AttendDatabase.Models;
public class NsiStudent
{
    public Guid Id { get; set; }
    public required string PersonalNumber { get; set; }

    public Guid NsiHumanId { get; set; }
    public NsiHuman? NsiHuman { get; set; }

    public ICollection<StudentMembership> StudentMemberships { get; set; } = new List<StudentMembership>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}
