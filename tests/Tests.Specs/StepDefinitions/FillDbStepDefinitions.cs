using System.Globalization;

using AttendDatabase;

using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;

using RTUAttendAPI.AttendDatabase.Models;

using TechTalk.SpecFlow.Assist;

namespace Tests.Specs.StepDefinitions;

public class ObjectsStore
{
    private readonly Dictionary<(Type, string), object> _objects = new();
    private readonly Dictionary<Type, object> _latest = new();

    public void Save<T>(string key, T value) where T : class
    {
        _objects[(typeof(T), key)] = value;
        _latest[typeof(T)] = value;
    }
    public void Save<T>(T value) where T : class
    {
        _objects[(typeof(T), "")] = value;
        _latest[typeof(T)] = value;
    }
    public T? Get<T>(string key) where T : class
    {
        var dKey = (typeof(T), key);
        if (!_objects.TryGetValue(dKey, out object? value))
        {
            return null;
        }
        _latest[typeof(T)] = value!;
        return value as T;
    }

    public T Last<T>() where T : class
    {
        return (_latest[typeof(T)] as T)!;
    }
}

[Binding]
public class FillDbStepDefinitions
{
    private readonly AttendDbContext _attendDbContext;
    private readonly ObjectsStore _objectsStore;
    private readonly HttpClient _httpClient;
    private Semester _semester = default!;

    public FillDbStepDefinitions(AttendDbContext attendDbContext, ObjectsStore objectsStore, HttpClient httpClient)
    {
        _attendDbContext = attendDbContext;
        _objectsStore = objectsStore;
        _httpClient = httpClient;
    }

    private async Task<T> GetOrCreate<T>(string key, Func<T> creator) where T : class
    {
        if (_objectsStore.Get<T>(key) is null)
        {
            var entry = creator();
            _attendDbContext.Add(entry);
            await _attendDbContext.SaveChangesAsync();
            _objectsStore.Save(key, entry);
        }
        return _objectsStore.Get<T>(key)!;
    }

    [Given("a global semester named (.*)")]
    public async Task GivenTheUserUsrWithElderRoleOnVisitingLogVl(string semesterTitle)
    {
        var fromDb = await _attendDbContext.Semesters.SingleOrDefaultAsync(s => s.Title == semesterTitle);
        if (fromDb is null)
        {
            _semester = await GetOrCreate(semesterTitle, () => new Semester
            {
                Title = semesterTitle,
                Start = DateTimeOffset.UtcNow,
                End = DateTimeOffset.UtcNow + TimeSpan.FromDays(30),
            });
        }
        else
        {
            _semester = fromDb;
            _objectsStore.Save(_semester);
        }
    }


    [Given(@"the student (.*)")]
    public async Task GivenTheStudent(string studentId)
    {
        var student = await GetOrCreate(studentId, () => new NsiStudent
        {
            NsiHuman = new NsiHuman
            {
                Firstname = studentId,
                Lastname = studentId,
                Middlename = studentId,
            },
            PersonalNumber = studentId,
        });
        await AuthAsHuman(student.NsiHuman!);
    }

    [Given(@"the teacher (.*)")]
    public async Task GivenTheTeacher(string teacherLabel)
    {
        var teacher = await GetOrCreateTeacher(teacherLabel);
        await AuthAsHuman(teacher.NsiHuman!);
    }

    private async Task<Teacher> GetOrCreateTeacher(string teacherLabel)
    {
        return await GetOrCreate(teacherLabel, () => new Teacher
        {
            NsiHuman = new NsiHuman
            {
                Firstname = teacherLabel,
                Lastname = teacherLabel,
                Middlename = teacherLabel,
            },
        });
    }

    private async Task AuthAsHuman(NsiHuman human)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, "/debug-login");
        message.Headers.Add("human-id", human.Id.ToString());
        var response = await _httpClient.SendAsync(message);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add(HeaderNames.Cookie, response.Headers.GetValues(HeaderNames.SetCookie)); // так как почему-то не работает работа встроенного механизма сохранения Cookie
        response.EnsureSuccessStatusCode();
    }

    [Given(@"(.*) is elder for (.*)")]
    public async Task GivenIsElderForVl(string studentId, string visitingLogTitle)
    {
        var log = new StudentMembership
        {
            MembershipRole = RTUAttendAPI.AttendDatabase.Models.StudentMembershipRole.Elder,
            MembershipType = RTUAttendAPI.AttendDatabase.Models.StudentMembershipType.Active,
            NsiStudent = _objectsStore.Get<NsiStudent>(studentId),
            VisitingLog = _objectsStore.Get<VisitingLog>(visitingLogTitle),
        };
        _attendDbContext.Add(log);
        await _attendDbContext.SaveChangesAsync();
    }
    public class CreateVLWithStudentsRecord
    {
        public string VisitingLogTitle { get; set; } = default!;
        public string StudentId { get; set; } = default!;
        public RTUAttendAPI.AttendDatabase.Models.StudentMembershipRole Status { get; set; }
    }

    public class CreateVlWithDisciplinesRecord
    {
        public string VisitingLogTitle { get; set; } = default!;
        public string Discipline { get; set; } = default!;
    }

    public class CreateLessonTypesRecord
    {
        public string LessonTypeName { get; set; } = default!;
    }

    [Given(@"exists visiting log with students")]
    public async Task GivenExistsVisitingLogWithStudents(Table table)
    {
        var links = table.CreateSet<CreateVLWithStudentsRecord>();
        foreach (var row in links)
        {
            var vl = await GetOrCreate(row.VisitingLogTitle, () => new VisitingLog
            {
                Title = row.VisitingLogTitle,
                IsArchived = false,
                Semester = _semester
            });
            var st = await GetOrCreate(row.StudentId, () => new NsiStudent
            {
                NsiHuman = new NsiHuman
                {
                    Firstname = row.StudentId,
                    Lastname = row.StudentId,
                    Middlename = row.StudentId,
                },
                PersonalNumber = row.StudentId,
            });
            _attendDbContext.Add(new StudentMembership
            {
                MembershipRole = row.Status,
                MembershipType = RTUAttendAPI.AttendDatabase.Models.StudentMembershipType.Active,
                NsiStudent = st,
                VisitingLog = vl,
            });
            await _attendDbContext.SaveChangesAsync();
        }
    }


    [Given(@"exists visiting log with disciplines")]
    public async Task GivenExistsVisitingLogWithDisciplines(Table table)
    {
        var links = table.CreateSet<CreateVlWithDisciplinesRecord>();
        foreach (var row in links)
        {
            var vl = await GetOrCreate(row.VisitingLogTitle, () => new VisitingLog
            {
                Title = row.VisitingLogTitle,
                IsArchived = false,
                Semester = _semester
            });
            var dc = await GetOrCreate(row.Discipline, () => new Discipline
            {
                Title = row.Discipline
            });
            await _attendDbContext.SaveChangesAsync();
        }
    }

    [Given(@"timeslot (.*) from (.*) to (.*)")]
    public async Task GivenTimeslotFromTo(string label, int timeSlotStartMinutes, int timeSlotEndMinutes)
    {
        await GetOrCreate(label, () => new TimeSlot
        {
            StartMinutesUTC = timeSlotStartMinutes,
            EndMinutesUTC = timeSlotEndMinutes,
            Semester = _semester,
        });
    }

    [Given(@"exists lesson types with statuses")]
    public async Task GivenExistsLessonTypesWithStatuses(Table table)
    {
        var links = table.CreateSet<CreateLessonTypesRecord>();
        foreach (var row in links)
        {
            var lt = await GetOrCreate(row.LessonTypeName, () => new LessonType
            {
                LessonTypeName = row.LessonTypeName
            });
            await _attendDbContext.SaveChangesAsync();
        }
    }

    [Given(@"exists lesson (.*) about (.*) at (.*) from (.*) to (.*) of type (.*)")]
    public async Task GivenExistsLessonAboutAtIn(string lessonLabel, string disciplineLabel, string dateStr, string fromStr, string toStr, string typeLabel)
    {
        var dateTime = DateOnly.Parse(dateStr, CultureInfo.InvariantCulture);
        var date = new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, TimeSpan.Zero);
        var startTime = TimeSpan.Parse(fromStr, CultureInfo.InvariantCulture) - TimeSpan.FromHours(3);
        var endTime = TimeSpan.Parse(toStr, CultureInfo.InvariantCulture) - TimeSpan.FromHours(3);
        var start = date + startTime;
        var end = date + startTime;
        var discipline = await GetOrCreate(disciplineLabel, () => new Discipline
        {
            Title = disciplineLabel,
        });
        var lessonType = await GetOrCreate(typeLabel, () => new LessonType
        {
            LessonTypeName = typeLabel
        });
        var lesson = await GetOrCreate(lessonLabel, () => new Lesson
        {
            Start = start,
            End = end,
            DisciplineId = discipline.Id,
            LessonTypeId = lessonType.Id,
        });
    }

    [Given(@"log (.*) contains lessons")]
    public async Task GivenLogContains(string visitingLogLabel, Table lessonLabelsTable)
    {
        var vl = await GetOrCreate(visitingLogLabel, () => new VisitingLog
        {
            Title = visitingLogLabel,
            IsArchived = false,
            Semester = _semester
        });
        var lessons = lessonLabelsTable.Rows.Select(r => r.Single().Value).Select((label) => _objectsStore.Get<Lesson>(label)!);

        foreach (var item in lessons)
        {
            var link = new VisitingLogLesson
            {
                VisitingLogId = vl.Id,
                LessonId = item.Id,
            };
            _attendDbContext.VisitingLogLessons.Add(link);
        }
        await _attendDbContext.SaveChangesAsync();
    }


    [Given(@"teacher (.*) leading to")]
    public async Task GivenTeacherLeadingTo(string teacherLabel, Table lessonLabelsTable)
    {
        var teacher = await GetOrCreateTeacher(teacherLabel);
        var lessons = lessonLabelsTable.Rows.Select(r => r.Single().Value).Select((label) => _objectsStore.Get<Lesson>(label)!);
        foreach (var item in lessons)
        {
            var link = new TeacherLesson
            {
                TeacherId = teacher.Id,
                LessonId = item.Id,
            };
            _attendDbContext.TeacherLessons.Add(link);
        }
        await _attendDbContext.SaveChangesAsync();
    }

}
