using AttendDatabase;

using Microsoft.EntityFrameworkCore;

using Quartz;

using RTUAttendAPI.API.Services.ExternalSerices;
using RTUAttendAPI.AttendDatabase.Models;

namespace RTUAttendAPI.API.Services.BackgroundSchedule;

[DisallowConcurrentExecution]
public class BackgroundScheduleJob : IJob, IDisposable
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IScheduleService _scheduleService;
    private readonly AttendDbContext _attendDbContext;
    private readonly ILogger<BackgroundScheduleJob> _logger;

    public BackgroundScheduleJob(
        IServiceScopeFactory serviceScopeFactory,
        IBackgroundJobScheduler backgroundJobScheduler,
        IScheduleService scheduleService,
        AttendDbContext attendDbContext,
        ILogger<BackgroundScheduleJob> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _backgroundJobScheduler = backgroundJobScheduler;
        _scheduleService = scheduleService;
        _attendDbContext = attendDbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await foreach (var schedule in _scheduleService.GetAllGroupSchedules())
        {
            // ОБЯЗАТЕЛЬНО учитываем факт того, что для этой строки начало нового дня - наши 3 часа ночи, поэтому и для семестра его таковым обозначаем.
            var scheduleUtcTime = DateTime.SpecifyKind(schedule.FirstDay.Date, DateTimeKind.Utc);
            var searchingVisitingLog = _attendDbContext.VisitingLogs
                .Where(l => l.Title == schedule.TargetTitle &&
                    l.Semester!.Start == scheduleUtcTime)
                .AsNoTracking()
                .FirstOrDefault();
            if (searchingVisitingLog is null)
            {
                var semesterId = await GetOrCreateSemester(schedule.FirstDay);
                var newVisitingLog = new VisitingLog
                {
                    Title = schedule.TargetTitle!,
                    IsArchived = false,
                    SemesterId = semesterId,
                    Source = VisitingLogSource.Schedule
                };
                var entry = _attendDbContext.VisitingLogs.Add(newVisitingLog);
                await _attendDbContext.SaveChangesAsync();
                entry.State = EntityState.Detached;

                await _backgroundJobScheduler.FillVisitingLogCompositionFromAcademicGroup(schedule.TargetTitle!, semesterId);

                _logger.LogInformation($"New visiting log for {schedule.TargetTitle} has been created");
            }
            else
            {
                _logger.LogDebug($"Visiting log for {schedule.TargetTitle} already exists");
            }
        }
    }

    private async Task<Guid> GetOrCreateSemester(DateTimeOffset time)
    {
        var newTime = DateTime.SpecifyKind(time.Date, DateTimeKind.Utc);

        var searchedSemesterId = _attendDbContext.Semesters.Where(
            s => s.Start == newTime)
            .Select(s => s.Id)
            .FirstOrDefault();
        if (searchedSemesterId == default)
        {
            var newSemester = new Semester
            {
                Title = time.ToString(),
                Start = newTime,
                End = newTime.AddDays(120)
            };
            var entry = _attendDbContext.Semesters.Add(newSemester);
            await _attendDbContext.SaveChangesAsync();
            entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            _logger.LogInformation("New semester for starting time {StartSemesterTime} has been created", time);
            return newSemester.Id;
        }
        else
        {
            _logger.LogDebug("Found existing semester");
            return searchedSemesterId;
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Background schedule job disposing");
    }
}
