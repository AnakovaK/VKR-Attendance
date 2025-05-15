using System.Globalization;

using Google.Protobuf.WellKnownTypes;

using RTUAttendAPI.AttendDatabase.Models;

using RtuTc.RtuAttend.App;

namespace Tests.Specs.StepDefinitions.GrpcStepDefinitions;
[Binding]
internal class ElderGrpcStepDefinitions
{
    private readonly ElderService.ElderServiceClient _grpcClient;
    private readonly ObjectsStore _objectsStore;

    public ElderGrpcStepDefinitions(ElderService.ElderServiceClient grpcClient, ObjectsStore objectsStore)
    {
        _grpcClient = grpcClient;
        _objectsStore = objectsStore;
    }

    [When(@"receive available visiting logs")]
    public async Task WhenReceiveAvailableVisitingLogs()
    {

        var response = await _grpcClient.GetAvailableVisitingLogsAsync(new GetAvailableVisitingLogsRequest
        {
            UserId = _objectsStore.Last<NsiStudent>().Id.ToString()
        });
        _objectsStore.Save(response);
    }
    [When(@"receive composition from (.*)")]
    public async Task WhenReceiveCompositionFrom(string vlId)
    {
        var response = await _grpcClient.GetGroupCompositionAsync(new GetGroupCompositionRequest
        {
            VisitingLogId = _objectsStore.Get<VisitingLog>(vlId)!.Id.ToString()
        });
        _objectsStore.Save(response);
    }

    [When(@"creating lesson in (.*)")]
    public async Task WhenCreatingLessonResultFrom(string vlId)
    {
        var response = await _grpcClient.CreateLessonAsync(new CreateLessonRequest
        {
            UserId = _objectsStore.Last<NsiStudent>().Id.ToString(),
            VisitingLogId = _objectsStore.Get<VisitingLog>(vlId)!.Id.ToString(),
            LessonDate = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
            TimeSlotId = _objectsStore.Last<TimeSlot>().Id.ToString(),
            DisciplineId = _objectsStore.Last<Discipline>().Id.ToString(),
            LessonTypeId = _objectsStore.Last<LessonType>().Id.ToString()
        });
        _objectsStore.Save(response);
    }

    [When(@"receive lessons in (.*) for (.*)")]
    public async Task WhenReceiveLessonsFromFor(string visitingLogLabel, string dateStr)
    {
        var targetDate = DateTime.Parse(dateStr, CultureInfo.InvariantCulture);
        targetDate = DateTime.SpecifyKind(targetDate, DateTimeKind.Utc);
        var response = await _grpcClient.GetAvailableLessonsAsync(new GetAvailableLessonsRequest
        {
            VisitingLogId = _objectsStore.Get<VisitingLog>(visitingLogLabel)!.Id.ToString(),
            Date = new()
            {
                Year = targetDate.Year,
                Month = targetDate.Month,
                Day = targetDate.Day
            }
        });
        _objectsStore.Save(response);
    }

}
