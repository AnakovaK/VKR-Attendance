using System.Diagnostics;

using Ardalis.Specification.EntityFrameworkCore;

using AttendDatabase;

using Ical.Net.CalendarComponents;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Quartz;

using RTUAttendAPI.API.Configuration;
using RTUAttendAPI.API.Extensions;
using RTUAttendAPI.API.Services.ExternalSerices;
using RTUAttendAPI.AttendDatabase.Models;
using RTUAttendAPI.AttendDatabase.Specs;

using Auditorium = RTUAttendAPI.AttendDatabase.Models.Auditorium;
using Calendar = Ical.Net.Calendar;
using Discipline = RTUAttendAPI.AttendDatabase.Models.Discipline;
using NsiHuman = RTUAttendAPI.AttendDatabase.Models.NsiHuman;
using Teacher = RTUAttendAPI.AttendDatabase.Models.Teacher;

namespace RTUAttendAPI.API.Services.BackgroundSchedule;

[DisallowConcurrentExecution]
public class BackgroundDailyScheduleActualizeJob : IJob, IDisposable
{

    private readonly HttpClient _httpClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly AttendDbContext _attendDbContext;
    private readonly IScheduleService _scheduleService;
    private readonly IOptions<CreateLessonFromScheduleOptions> _options;
    private readonly ILogger<BackgroundDailyScheduleActualizeJob> _logger;
    public BackgroundDailyScheduleActualizeJob(
        HttpClient httpClient,
        IServiceScopeFactory serviceScopeFactory,
        AttendDbContext attendDbContext, // TODO: разнести выборки и изменения в разные grpc сервисы (мини CQRS)
        IScheduleService scheduleService,
        IOptions<CreateLessonFromScheduleOptions> options,
        ILogger<BackgroundDailyScheduleActualizeJob> logger)
    {
        _httpClient = httpClient;
        _serviceScopeFactory = serviceScopeFactory;
        _attendDbContext = attendDbContext;
        _scheduleService = scheduleService;
        _options = options;
        _logger = logger;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        await foreach (var schedule in _scheduleService.GetAllGroupSchedules())
        {
            var calendar = Calendar.Load(await _httpClient.GetStreamAsync(schedule.ICalLink));

            var utcNow = DateTime.UtcNow;

            var searchStart = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 00, 00, DateTimeKind.Utc);
            var searchEnd = searchStart.Add(_options.Value.RangeToScan);

            var occurrences = calendar.GetOccurrences(searchStart, searchEnd);
            foreach (var occurrence in occurrences)
            {
                var iCalEvent = occurrence.Source as CalendarEvent
                    ?? throw new UnreachableException($"occurrence source has unexpected type {occurrence.Source.GetType().FullName}");
                var lessonStart = occurrence.Period.StartTime.AsDateTimeOffset.ToUniversalTime();
                var lessonEnd = occurrence.Period.EndTime.AsDateTimeOffset.ToUniversalTime();

                var existsLesson = await _attendDbContext.Lessons
                    .WithSpecification(new FindLessonBasedOnSchedule(lessonStart, iCalEvent.Uid.AsGuid()))
                    .SingleOrDefaultAsync();

                if (existsLesson is null)
                {
                    await CreateLesson(iCalEvent.Uid.AsGuid(), lessonStart, lessonEnd);
                    _logger.LogTrace("Lesson exists on time {LessonStart} for group {TargetTitle}", lessonStart, schedule.TargetTitle);
                }
                else
                {
                }
            }
            _logger.LogInformation("Occurrences: {Occurrences}", occurrences);
        }
    }

    public void Dispose()
    {
        _logger.LogInformation("Background daily schedule actualize job disposing");
    }

    public async Task CreateLesson(Guid eventId, DateTimeOffset lessonStart, DateTimeOffset lessonEnd)
    {
        var eventInfo = await _scheduleService.GetEventProperties(eventId);

        var disciplineId = await GetOrCreateDiscipline(eventInfo!.DisciplineTitle, eventInfo.GroupTitles);
        var lessonTypeId = await GetOrCreateLessonType(eventInfo!.LessonTypeName);
        var newLesson = new Lesson
        {
            DisciplineId = disciplineId,
            LessonTypeId = lessonTypeId,
            Start = lessonStart,
            End = lessonEnd,
            CreatedFromScheduleEventId = eventId
        };

        foreach (var location in eventInfo.Auditoriums)
        {
            newLesson.Auditoriums.Add(new AuditoriumLesson
            {
                AuditoriumId = await GetOrCreateAuditorium(location),
                Lesson = newLesson
            });
        }

        foreach (var teacher in eventInfo.Teachers)
        {
            var newTeacherLesson = new TeacherLesson
            {
                TeacherId = await GetOrCreateTeacher(teacher),
                Lesson = newLesson
            };
            newLesson.Teachers.Add(newTeacherLesson);
            _attendDbContext.TeacherLessons.Add(newTeacherLesson);

        }

        foreach (var visitingLogTitle in eventInfo.GroupTitles)
        {
            var currentVisitingLogInDbId = await GetVisitingLogId(visitingLogTitle);
            if (!currentVisitingLogInDbId.HasValue)
            {
                _logger.LogWarning("Not found visiting log for group {VisitingLogTitle}", visitingLogTitle);
                continue;
            }
            var lessonToLogLink = new VisitingLogLesson
            {
                Lesson = newLesson,
                VisitingLogId = currentVisitingLogInDbId.Value
            };
            newLesson.VisitingLogs.Add(lessonToLogLink);
            _attendDbContext.VisitingLogLessons.Add(lessonToLogLink);
        }

        _attendDbContext.Lessons.Add(newLesson);
        await _attendDbContext.SaveChangesAsync();
        _logger.LogInformation("New lesson for discipline {Discipline} on time {lessonStart} was added", disciplineId, lessonStart);
    }

    public async Task<Guid> GetOrCreateDiscipline(string disciplineTitle, List<string> groupTitles)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AttendDbContext>();

        var searchedDisciplinesId = await db.Disciplines.Where(
                d => d.Title == disciplineTitle)
            .Select(d => d.Id)
            .FirstOrDefaultAsync();

        if (searchedDisciplinesId != default)
        {
            _logger.LogTrace("Found existing discipline");
            return searchedDisciplinesId;
        }
        var newDiscipline = new Discipline
        {
            Title = disciplineTitle,
        };
        db.Disciplines.Add(newDiscipline);
        foreach (var groupTitle in groupTitles)
        {
            var visitingLogId = await GetVisitingLogId(groupTitle);
            if (!visitingLogId.HasValue)
            {
                _logger.LogWarning("Not found visiting log for group {VisitingLogTitle}", visitingLogId);
                continue;
            }
            var newDisciplineLogLink = new DisciplineVisitingLog
            {
                FkDisciplineVisitingLog = newDiscipline,
                FkVisitingLogId = visitingLogId.Value
            };
            db.DisciplinesVisitingLogs.Add(newDisciplineLogLink);
        }
        await db.SaveChangesAsync();
        _logger.LogInformation("New discipline {DisciplineTitle} was added", disciplineTitle);
        return newDiscipline.Id;

    }

    public async Task<Guid> GetOrCreateLessonType(string lessonTypeName)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AttendDbContext>();

        var searchedLessonTypeId = db.LessonTypes.Where(
            l => l.LessonTypeName == lessonTypeName)
            .Select(l => l.Id)
            .FirstOrDefault();
        if (searchedLessonTypeId == default)
        {
            var newLessonType = new LessonType
            {
                LessonTypeName = lessonTypeName
            };
            db.LessonTypes.Add(newLessonType);
            await db.SaveChangesAsync();
            _logger.LogInformation("New lesson type {LessonType} was added", newLessonType.LessonTypeName);
            return newLessonType.Id;
        }
        else
        {
            return searchedLessonTypeId;
        }
    }

    public async Task<Guid?> GetVisitingLogId(string visitingLogTitle)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AttendDbContext>();

        var searchedVisitingLogId = await db.VisitingLogs.Where( //TODO: теперь есть спека, делающая это, ВЫНЕСТИ
            vl => vl.Title == visitingLogTitle)
            .Where(vl => !vl.IsArchived)
            .Select(vl => vl.Id)
            .FirstOrDefaultAsync();
        if (searchedVisitingLogId == default)
        {
            return null;
        }
        return searchedVisitingLogId;
    }

    public async Task<Guid> GetOrCreateTeacher(ScheduleTeacher teacher)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AttendDbContext>();

        var searchedHumanId = db.NsiHumans.Where(
            h => h.Id == teacher.HumanId)
            .Select(h => h.Id)
            .FirstOrDefault();

        if (searchedHumanId == default)
        {
            var newHuman = new NsiHuman
            {
                Id = teacher.HumanId,
                Firstname = teacher.Firstname,
                Lastname = teacher.Lastname,
                Middlename = teacher.Middlename
            };
            db.NsiHumans.Add(newHuman);
            searchedHumanId = teacher.HumanId;
        }

        var searchedTeacherId = db.Teachers.Where(
                t => t.NsiHumanId == searchedHumanId)
            .Select(t => t.Id)
            .FirstOrDefault();

        if (searchedTeacherId == default)
        {
            var newTeacher = new Teacher
            {
                NsiHumanId = searchedHumanId
            };
            db.Teachers.Add(newTeacher);
            await db.SaveChangesAsync();
            return newTeacher.Id;
        }
        return searchedTeacherId;
    }

    public async Task<Guid> GetOrCreateAuditorium(ScheduleAuditorium auditorium)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AttendDbContext>();

        var searchedAuditoriumId = db.Auditoriums.Where(
                a => a.Number == auditorium.Number && a.Campus == auditorium.Building)
            .Select(h => h.Id)
            .FirstOrDefault();
        if (searchedAuditoriumId == default)
        {
            var newAuditorium = new Auditorium
            {
                Number = auditorium.Number,
                Campus = auditorium.Building
            };
            db.Auditoriums.Add(newAuditorium);
            await db.SaveChangesAsync();
            return newAuditorium.Id;
        }
        return searchedAuditoriumId;
    }
}
