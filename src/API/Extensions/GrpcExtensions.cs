using RTUAttendAPI.AttendDatabase.Models;

public static class GrpcExtensions
{
    internal static RtuTc.RtuAttend.App.StudentMembershipType ToGrpc(this StudentMembershipType studentMembershipType)
        => studentMembershipType switch
        {
            StudentMembershipType.Unknown => RtuTc.RtuAttend.App.StudentMembershipType.Unknown,
            StudentMembershipType.Active => RtuTc.RtuAttend.App.StudentMembershipType.Active,
            StudentMembershipType.Inactive => RtuTc.RtuAttend.App.StudentMembershipType.Inactive,
            _ => throw new ArgumentException($"Incorrect type {studentMembershipType}", nameof(studentMembershipType))
        };
    internal static RtuTc.RtuAttend.App.StudentMembershipRole ToGrpc(this StudentMembershipRole studentMembershipRole)
        => studentMembershipRole switch
        {
            StudentMembershipRole.Unknown => RtuTc.RtuAttend.App.StudentMembershipRole.Unknown,
            StudentMembershipRole.Student => RtuTc.RtuAttend.App.StudentMembershipRole.Student,
            StudentMembershipRole.Elder => RtuTc.RtuAttend.App.StudentMembershipRole.Elder,
            StudentMembershipRole.ViceElder => RtuTc.RtuAttend.App.StudentMembershipRole.ViceElder,
            _ => throw new ArgumentException($"Incorrect role {studentMembershipRole}", nameof(studentMembershipRole))
        };

    internal static RtuTc.RtuAttend.Models.StudentMembershipType ToModelGrpc(this StudentMembershipType studentMembershipType)
    => studentMembershipType switch
    {
        StudentMembershipType.Unknown => RtuTc.RtuAttend.Models.StudentMembershipType.Unknown,
        StudentMembershipType.Active => RtuTc.RtuAttend.Models.StudentMembershipType.Active,
        StudentMembershipType.Inactive => RtuTc.RtuAttend.Models.StudentMembershipType.Inactive,
        _ => throw new ArgumentException($"Incorrect type {studentMembershipType}", nameof(studentMembershipType))
    };

    internal static RtuTc.RtuAttend.Models.StudentMembershipRole ToModelGrpc(this StudentMembershipRole studentMembershipRole)
    => studentMembershipRole switch
    {
        StudentMembershipRole.Unknown => RtuTc.RtuAttend.Models.StudentMembershipRole.Unknown,
        StudentMembershipRole.Student => RtuTc.RtuAttend.Models.StudentMembershipRole.Student,
        StudentMembershipRole.Elder => RtuTc.RtuAttend.Models.StudentMembershipRole.Elder,
        StudentMembershipRole.ViceElder => RtuTc.RtuAttend.Models.StudentMembershipRole.ViceElder,
        _ => throw new ArgumentException($"Incorrect role {studentMembershipRole}", nameof(studentMembershipRole))
    };

    internal static RtuTc.RtuAttend.Models.AttendType ToGrpc(this AttendType attendType) => attendType switch
    {
        AttendType.Unknown => RtuTc.RtuAttend.Models.AttendType.Unknown,
        AttendType.Absent => RtuTc.RtuAttend.Models.AttendType.Absent,
        AttendType.ExcusedAbsence => RtuTc.RtuAttend.Models.AttendType.ExcusedAbsence,
        AttendType.Present => RtuTc.RtuAttend.Models.AttendType.Present,
        _ => throw new ArgumentException($"Incorrect attend type {attendType}", nameof(attendType))
    };
    internal static AttendType FromGrpc(this RtuTc.RtuAttend.Models.AttendType attendType) => attendType switch
    {
        RtuTc.RtuAttend.Models.AttendType.Unknown => AttendType.Unknown,
        RtuTc.RtuAttend.Models.AttendType.Absent => AttendType.Absent,
        RtuTc.RtuAttend.Models.AttendType.ExcusedAbsence => AttendType.ExcusedAbsence,
        RtuTc.RtuAttend.Models.AttendType.Present => AttendType.Present,
        _ => throw new ArgumentException($"Incorrect attend type {attendType}", nameof(attendType))
    };

    internal static StudentMembershipRole FromGrpc(this RtuTc.RtuAttend.Models.StudentMembershipRole role) => role switch
    {
        RtuTc.RtuAttend.Models.StudentMembershipRole.Unknown => StudentMembershipRole.Unknown,
        RtuTc.RtuAttend.Models.StudentMembershipRole.Student => StudentMembershipRole.Student,
        RtuTc.RtuAttend.Models.StudentMembershipRole.Elder => StudentMembershipRole.Elder,
        RtuTc.RtuAttend.Models.StudentMembershipRole.ViceElder => StudentMembershipRole.ViceElder,
        _ => throw new ArgumentException($"Incorrect student membership role {role}", nameof(role))
    };

    public static Google.Type.Date ToGrpc(this DateOnly date) => new()
    {
        Year = date.Year,
        Month = date.Month,
        Day = date.Day,
    };
    internal static DateOnly ToDateOnly(this Google.Type.Date date) => new(date.Year, date.Month, date.Day);
}
