using System.Diagnostics;
using System.Net;
using System.Text.Json.Serialization;

using RTUAttendAPI.API.Services.BackgroundSchedule;

namespace RTUAttendAPI.API.Services.ExternalSerices;

public interface IScheduleService
{
    IAsyncEnumerable<Schedule> GetAllGroupSchedules();
    public Task<ICalEventProperties> GetEventProperties(Guid icalEventId);
}

public class HttpRTUSchedule : IScheduleService
{
    private readonly HttpClient _httpClient;

    public HttpRTUSchedule(HttpClient httpClient, ILogger<HttpRTUSchedule> logger)
    {
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<Schedule> GetAllGroupSchedules()
    {
        string? pageToken = null;
        do
        {
            var currentList = await _httpClient.GetFromJsonAsync<PaginationParams>($"/schedule/api/search?limit=50&pageToken={WebUtility.UrlEncode(pageToken)}")
                ?? throw new UnreachableException("Can't retrieve schedule");
            foreach (var item in currentList.Schedules.Where(s => s.ScheduleTarget == 1))
            {
                yield return item;
            }
            pageToken = currentList.NextPageToken;
        } while (pageToken is not null);
    }

    public async Task<ICalEventProperties> GetEventProperties(Guid icalEventId)
    {
        return await _httpClient.GetFromJsonAsync<ICalEventProperties>($"/schedule/api/internal/icaleventproperties?icalEventId={icalEventId}")
            ?? throw new UnreachableException($"Can't receive ical properties for event {icalEventId}");
    }
}

public class ICalEventProperties
{
    [JsonPropertyName("discipline")]
    public string DisciplineTitle { get; set; } = default!;
    [JsonPropertyName("lessonType")]
    public string LessonTypeName { get; set; } = default!;

    [JsonPropertyName("groups")]
    public List<string> GroupTitles { get; set; } = new List<string>();
    public List<ScheduleAuditorium> Auditoriums { get; set; } = new List<ScheduleAuditorium>();
    public List<ScheduleTeacher> Teachers { get; set; } = new List<ScheduleTeacher>();
}

public class ScheduleAuditorium
{
    [JsonPropertyName("number")]
    public string Number { get; set; } = default!;

    [JsonPropertyName("building")]
    public string Building { get; set; } = default!;
}

public class ScheduleTeacher
{
    [JsonPropertyName("tandemId")]
    public Guid HumanId { get; set; }
    [JsonPropertyName("lastname")]
    public string Lastname { get; set; } = default!;
    [JsonPropertyName("firstname")]
    public string Firstname { get; set; } = default!;
    [JsonPropertyName("middlename")]
    public string Middlename { get; set; } = default!;
}
