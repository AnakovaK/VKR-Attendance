using System.Text.Json.Serialization;
namespace RTUAttendAPI.API.Services.BackgroundSchedule;

public class Schedule
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("targetTitle")]
    public string TargetTitle { get; set; } = default!;
    [JsonPropertyName("scheduleTarget")]
    public int ScheduleTarget { get; set; }
    [JsonPropertyName("iCalLink")]
    public string ICalLink { get; set; } = default!;
    // TODO: или доделать API расписания, или вынести как конфигурационный параметр
    public DateTimeOffset FirstDay => new(new DateTime(2023, 09, 01), TimeSpan.FromHours(3));
}
