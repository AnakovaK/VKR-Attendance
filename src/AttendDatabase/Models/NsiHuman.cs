namespace RTUAttendAPI.AttendDatabase.Models;
public class NsiHuman
{
    public Guid Id { get; set; }
    public required string Lastname { get; set; }
    public required string Firstname { get; set; }
    public required string? Middlename { get; set; }

    public ICollection<NsiStudent> NsiStudents { get; set; } = new List<NsiStudent>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    public ICollection<NsiHumanLoginId> Logins { get; set; } = new List<NsiHumanLoginId>();
}
