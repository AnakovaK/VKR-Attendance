using System.Text.Json.Serialization;
using Microsoft.VisualBasic;

namespace RTUAttendAPI.API.Services.BackgroundSchedule;

public class PaginationParams
{
    [JsonPropertyName("data")]
    public List<Schedule> Schedules { get; set; } = new List<Schedule>();
    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}
