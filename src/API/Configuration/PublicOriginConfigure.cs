using Microsoft.Extensions.DependencyInjection;

namespace RTUAttendAPI.API.Configuration;

public class PublicOriginOptions
{
    public required string Uri { get; set; }
}

public static class PublicOriginConfigure
{
    public static void ConfigurePublicOrigin(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<PublicOriginOptions>(builder.Configuration.GetSection("PublicOrigin"));
    }
}
