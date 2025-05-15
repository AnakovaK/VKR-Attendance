namespace RTUAttendAPI.API.Configuration;

public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

public static class CorsConfigure
{
    public const string CorsPolicyName = "Default";
    public static void ConfigureCors(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection("Cors"));
        builder.Services.AddCors(options =>
        {
            var corsOptions = new CorsOptions();
            builder.Configuration.GetSection("Cors").Bind(corsOptions);
            options.AddPolicy(CorsPolicyName, policy => policy.WithOrigins(corsOptions.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                    .WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding"));
        });
    }
}
