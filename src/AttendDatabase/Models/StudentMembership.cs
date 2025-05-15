namespace RTUAttendAPI.AttendDatabase.Models;
public class StudentMembership
{
    public Guid Id { get; set; }
    /// <summary>
    /// Завязка на студента будет при интеграции с тандемом, сейчас это просто поле, которое можно будет заполнить в будущем
    /// </summary>
    public Guid NsiStudentId { get; set; }
    public NsiStudent? NsiStudent { get; set; }

    public Guid VisitingLogId { get; set; }
    public VisitingLog? VisitingLog { get; set; }

    public required StudentMembershipType MembershipType { get; set; }
    public required StudentMembershipRole MembershipRole { get; set; }
}
public enum StudentMembershipType
{
    /// <summary>
    /// Неизвестный тип
    /// </summary>
    Unknown,
    /// <summary>
    /// Активный член группы
    /// </summary>
    Active,
    /// <summary>
    /// Неактивный член группы (переведен в другую, отчислен)
    /// </summary>
    Inactive
}
public enum StudentMembershipRole
{
    /// <summary>
    /// Неизвестная роль
    /// </summary>
    Unknown,
    /// <summary>
    /// Рядовой студент
    /// </summary>
    Student,
    /// <summary>
    /// Староста
    /// </summary>
    Elder,
    /// <summary>
    /// Заместитель старосты
    /// </summary>
    ViceElder
}

