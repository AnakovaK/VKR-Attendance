using Quartz;

namespace RTUAttendAPI.API.Services.BackgroundSchedule;

public interface IBackgroundJobScheduler
{
    Task FillVisitingLogCompositionFromAcademicGroup(string groupTitle, Guid semesterId);
}


public sealed class QuartzBackgroundJobScheduler : IBackgroundJobScheduler
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<QuartzBackgroundJobScheduler> _logger;

    public QuartzBackgroundJobScheduler(
            ISchedulerFactory schedulerFactory,
            ILogger<QuartzBackgroundJobScheduler> logger
    )
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task FillVisitingLogCompositionFromAcademicGroup(string groupTitle, Guid semesterId)
    {
        var jobKey = new JobKey("Actualize Group Job");

        var jobData = new JobDataMap
        {
            { "givenGroup", groupTitle },
            { "currentSemester", semesterId.ToString() }
        };

        var scheduler = await _schedulerFactory.GetScheduler();
        await scheduler.TriggerJob(jobKey, jobData);
    }
}
