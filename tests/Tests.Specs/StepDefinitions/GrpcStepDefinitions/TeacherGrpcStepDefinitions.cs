using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;

using RTUAttendAPI.AttendDatabase.Models;

using RtuTc.RtuAttend.App;

namespace Tests.Specs.StepDefinitions.GrpcStepDefinitions;
[Binding]
internal class TeacherGrpcStepDefinitions
{
    private readonly TeacherService.TeacherServiceClient _grpcClient;
    private readonly ObjectsStore _objectsStore;

    public TeacherGrpcStepDefinitions(TeacherService.TeacherServiceClient grpcClient, ObjectsStore objectsStore)
    {
        _grpcClient = grpcClient;
        _objectsStore = objectsStore;
    }

    [When(@"receive teacher lessons for (.*)")]
    public async Task WhenReceiveTeacherLessons(string dateStr)
    {
        var targetDate = DateOnly.Parse(dateStr, CultureInfo.InvariantCulture);
        var response = await _grpcClient.GetMyLessonsForDateAsync(new GetMyLessonsForDateRequest
        {
            Date = targetDate.ToGrpc(),
        });
        _objectsStore.Save(response);
    }

}
