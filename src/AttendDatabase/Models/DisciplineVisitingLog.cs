namespace RTUAttendAPI.AttendDatabase.Models;
public class DisciplineVisitingLog
{
    public Guid Id { get; set; }
    public Guid FkDisciplineId { get; set; }
    public Discipline? FkDisciplineVisitingLog { get; set; }
    public Guid FkVisitingLogId { get; set; }
    public VisitingLog? FkVisitingLogDiscipline { get; set; }
}
