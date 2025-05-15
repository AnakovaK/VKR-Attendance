using Quartz;

using RTUAttendAPI.API.Constants;
using RTUAttendAPI.API.Services.BackgroundSchedule;

namespace RTUAttendAPI.API.Configuration;

public static class QuartzConfigure
{
    public static void ConfigureBackgroundJobs(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<CreateLessonFromScheduleOptions>(builder.Configuration.GetSection("CreateLessonFromScheduleOptions"));
        builder.Services.AddQuartz(q =>
        {
            var actualizeGroupJobKey = new JobKey("Actualize Group Job");
            var dailyScheduleKey = new JobKey("Current Day Schedule Job");

            q.AddJob<BackgroundScheduleJob>(opts => 
                opts.WithIdentity(JobKeys.ACADEMIC_GROUPS_FETCH));

            q.AddJob<BackgroundGroupActualizeJob>(opts =>
                opts.WithIdentity(actualizeGroupJobKey)
                .StoreDurably(true));

            q.AddJob<BackgroundDailyScheduleActualizeJob>(opts =>
            opts.WithIdentity(dailyScheduleKey));

            q.AddTrigger(opts => opts
                .ForJob(JobKeys.ACADEMIC_GROUPS_FETCH)
                .WithIdentity("Simpler-trigger")
                .WithCronSchedule(builder.Configuration["BackgroundScheduleMode"]!)
            );

            q.AddTrigger(opts => opts
            .ForJob(dailyScheduleKey)
            .WithIdentity("Current-Day-Schedule-Tigger")
            .WithCronSchedule(builder.Configuration["CurrentDayScheduleMode"]!));

            if (builder.Configuration.GetConnectionString(ConnectionStrings.QuartzDatabase) is not null)
            {
                q.UsePersistentStore(s =>
                {
                    s.PerformSchemaValidation = true; // default
                    s.UseProperties = true; // preferred, but not default
                    s.RetryInterval = TimeSpan.FromSeconds(15);
                    s.UsePostgres(p =>
                    {
                        p.ConnectionString = builder.Configuration.GetConnectionString(ConnectionStrings.QuartzDatabase)!;
                        p.TablePrefix = "QRTZ_";
                    });
                    s.UseNewtonsoftJsonSerializer();
                    s.UseClustering(c =>
                    {
                        c.CheckinMisfireThreshold = TimeSpan.FromSeconds(20);
                        c.CheckinInterval = TimeSpan.FromSeconds(10);
                    });
                });
            }
        });
        builder.Services.AddQuartzHostedService(opt => opt.WaitForJobsToComplete = true);
    }
}
