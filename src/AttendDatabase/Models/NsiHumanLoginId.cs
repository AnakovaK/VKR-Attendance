using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTUAttendAPI.AttendDatabase.Models;
/// <summary>
/// Вспомогательная сущность, которая позволит входить аккаунтам через id преподавателей, не обращаясь за этой информацией в тандем
/// </summary>
public class NsiHumanLoginId
{
    /// <summary>
    /// id который вернулся из login
    /// </summary>
    public Guid LoginId { get; set; }
    public Guid HumanId { get; set; }
    public NsiHuman? Human { get; set; }
}
