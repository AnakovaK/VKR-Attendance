using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RTUAttendAPI.AttendDatabase.Models;
public class LoginEvent
{
    public Guid Id { get; set; }
    public DateTimeOffset Date { get; set; }
    public required JsonDocument LoginInfo { get; set; }
}
