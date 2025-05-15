namespace RTUAttendAPI.AttendDatabase.Models;
public class TimeSlot
{
    public Guid Id { get; set; }
    public required int StartMinutesUTC { get; set; }
    public required int EndMinutesUTC { get; set; }
    public Guid SemesterId { get; set; }
    public Semester? Semester { get; set; }
}
