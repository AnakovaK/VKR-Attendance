using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTUAttendAPI.AttendDatabase.Models;
public class Auditorium
{
    public Guid Id { get; set; }
    public required string Number { get; set; }
    public required string Campus { get; set; } // возможно вынесем в отдельную сущность

    public ICollection<AuditoriumLesson> Lessons { get; set; } = new List<AuditoriumLesson>();
}
