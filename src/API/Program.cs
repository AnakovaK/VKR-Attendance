using System.Reflection;

using API.Common;

using AttendDatabase;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

using RTUAttendAPI.API.Configuration;
using RTUAttendAPI.API.Services;
using RTUAttendAPI.API.Services.BackgroundSchedule;
using RTUAttendAPI.API.Services.BusinessLogic;
using RTUAttendAPI.API.Services.Grpc;
using RTUAttendAPI.API.Services.Initialization;
using RTUAttendAPI.AttendDatabase;
using RTUAttendAPI.API.Services.ExternalSerices;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseSentry();

builder.AddDistributedCache();

builder.Services.AddHttpClient<IScheduleService, HttpRTUSchedule>(c =>
{
    var baseAddress = new Uri(builder.Configuration.GetConnectionString(ConnectionStrings.BackgroundScheduleLink)!);
    c.BaseAddress = baseAddress;
});

builder.ConfigureBackgroundJobs();

builder.ConfigureCors();
builder.ConfigurePublicOrigin();
builder.Services.AddHostedService<MigrateDb>();

builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<IUniversityDataProvider, TandemGrpcProvider>();
//builder.Services.AddScoped<IUniversityDataProvider, StaticDataProvider>(); 
builder.Services.AddScoped<IBackgroundJobScheduler, QuartzBackgroundJobScheduler>();

builder.Services.AddHealthChecks()
    .AddCheck("version", () => HealthCheckResult.Healthy(Assembly.GetExecutingAssembly().GetName().Version!.ToString()))
    .AddNpgSql(sp => sp.GetRequiredService<IConfiguration>().GetConnectionString(ConnectionStrings.PostgresDatabase)!);

builder.Services.AddGrpcClients(builder.Configuration.GetConnectionString(ConnectionStrings.GrpcTandem)!);

builder.Services.AddAttendDb(() => builder.Configuration.GetConnectionString(ConnectionStrings.PostgresDatabase)!);
builder.Services.AddDataProtection()
       .PersistKeysToDbContext<AttendDbContext>()
       .UseCryptographicAlgorithms(
            new AuthenticatedEncryptorConfiguration
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            });

builder.ConfigureAuthentication();

builder.AddStudentSelfApproveLink();


var app = builder.Build();

app.MapHealthChecks("/healthcheck", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthCheck.CreateJsonResponse
});

app.Use((context, next) =>
{
    // TODO: configure MIREA machine for correct forward headers
    context.Request.Scheme = "https";
    return next();
});

app.MapGet("/api/login", (string redirectUri, IOptions<CorsOptions> options) =>
{
    if (options.Value.AllowedOrigins.All(o => !redirectUri.StartsWith(o, StringComparison.OrdinalIgnoreCase)))
    {
        return Results.BadRequest(new { error = "incorrect origin", url = redirectUri });
    }
    var props = new AuthenticationProperties
    {
        RedirectUri = redirectUri,
    };
    return Results.Challenge(props);
})
.WithName("login");

app.MapGet("/api/logout", (string redirectUri, IOptions<CorsOptions> options) =>
{
    if (options.Value.AllowedOrigins.All(o => !redirectUri.StartsWith(o, StringComparison.OrdinalIgnoreCase)))
    {
        return Results.BadRequest(new { error = "incorrect origin" });
    }
    var props = new AuthenticationProperties
    {
        RedirectUri = redirectUri,
    };
    return Results.SignOut(props);
})
.WithName("logout");

app.MapGet("claims", (HttpContext context) => Results.Ok(context.User.Claims.Select(c => new { c.Type, c.Value }))).RequireAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
    app.MapGet("debug-login", () => Results.Challenge(new AuthenticationProperties()));
}
app.UseStaticFiles();
app.UseRouting();
app.UseSentryTracing();
app.UseCors(CorsConfigure.CorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseGrpcWeb();
app.MapGrpcService<StudentService>().EnableGrpcWeb().RequireCors(CorsConfigure.CorsPolicyName);
app.MapGrpcService<ElderService>().EnableGrpcWeb().RequireCors(CorsConfigure.CorsPolicyName);
app.MapGrpcService<UserService>().EnableGrpcWeb().RequireCors(CorsConfigure.CorsPolicyName);
app.MapGrpcService<TeacherService>().EnableGrpcWeb().RequireCors(CorsConfigure.CorsPolicyName);
app.MapGrpcService<AdminService>().EnableGrpcWeb().RequireCors(CorsConfigure.CorsPolicyName);

await app.RunAsync();
public partial class Program { }
