using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RTUAttendAPI.AttendDatabase.Models;

using RtuTc.RtuAttend.App;

namespace Tests.Specs.StepDefinitions;
[Binding]
internal class ThenDefinitions
{
    private readonly ObjectsStore _objectsStore;

    public ThenDefinitions(ObjectsStore objectsStore)
    {
        _objectsStore = objectsStore;
    }

    [Then(@"received visiting log (.*)")]
    public void ThenReceivedVisitingLog(string visitingLogTitle)
    {
        _objectsStore.Last<AvailableVisitingLogsResponse>().Logs.Should().Contain(l => l.Id == _objectsStore.Get<VisitingLog>(visitingLogTitle)!.Id.ToString());
    }

    [Then(@"received composition contains")]
    public void ThenReceivedComposition(Table table)
    {
        var sts = table.Rows.Select(r => r.Single().Value).Select(_objectsStore.Get<NsiStudent>).Select(s => s!.Id.ToString());
        var memberships = _objectsStore.Last<GroupCompositionResponse>().Membership;
        memberships.Should()
            .OnlyContain(l => sts.Contains(l.StudentId));
    }

    [Then(@"lesson created successfully")]
    public void ThenRecievedCreateLessonResult()
    {
        var response = _objectsStore.Last<CreateLessonResponse>().Result;
        response.Should().Be(CreateLessonResult.Ok);
    }

    [Then(@"lessons received is")]
    public void ThenLessonsReceivedIs(Table lessonLabelsTable)
    {
        var lessons = lessonLabelsTable.Rows.Select(r => r.Single().Value).Select((label) => _objectsStore.Get<Lesson>(label)!);
        var response = _objectsStore.Last<AvailableLessonsResponse>().Lessons;

        response.Select(l => l.Id).Should().BeEquivalentTo(lessons.Select(l => l.Id.ToString()), config => config.WithStrictOrdering());
    }

    [Then(@"teacher lessons received is")]
    public void ThenTeacherLessonsReceivedIs(Table lessonLabelsTable)
    {
        var lessons = lessonLabelsTable.Rows.Select(r => r.Single().Value).Select((label) => _objectsStore.Get<Lesson>(label)!);
        var response = _objectsStore.Last<GetMyLessonsForDateResponse>().Lessons;

        response.Select(l => l.Id).Should().BeEquivalentTo(lessons.Select(l => l.Id.ToString()), config => config.WithStrictOrdering());
    }

}
