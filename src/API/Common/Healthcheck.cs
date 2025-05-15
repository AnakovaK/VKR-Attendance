using System.Net.Mime;
using System.Text.Json;

using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Common;

internal class HealthCheck
{
    public static async Task CreateJsonResponse(HttpContext httpContent, HealthReport report)
    {
        var json = JsonSerializer.Serialize(
            new
            {
                Status = report.Status.ToString(),
                Duration = report.TotalDuration,
                Info = report.Entries
                    .Select(e =>
                        new
                        {
                            Key = e.Key,
                            Description = e.Value.Description,
                            Duration = e.Value.Duration,
                            Status = Enum.GetName(e.Value.Status),
                            Error = e.Value.Exception?.Message,
                            Data = e.Value.Data
                        })
            });
        httpContent.Response.ContentType = MediaTypeNames.Application.Json;
        await httpContent.Response.WriteAsync(json);
    }
}
