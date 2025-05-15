using RTUAttendAPI.API.Services.BusinessLogic;

namespace RTUAttendAPI.API.Configuration;

public static class StudentSelfApproveLink
{
    public static void AddStudentSelfApproveLink(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<StudentSelfApproveLinkOptions>(builder.Configuration.GetSection("StudentSelfApproveLinkOptions"));
        builder.Services.AddScoped<IStudentSelfApproveLinksService, DistributedCacheApproveService>();
    }
}

public class StudentSelfApproveLinkOptions
{
    public TimeSpan DelayBetweenCreatingLinks { get; set; }
    public TimeSpan LinkLifetime { get; set; }
    public string LinkTemplate { get; set; } = default!;
}
