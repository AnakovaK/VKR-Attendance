using Google.Protobuf.WellKnownTypes;

using RTUAttendAPI.AttendDatabase.Specs;

using RtuTc.RtuAttend.Models;

namespace RTUAttendAPI.API.Services.Grpc;

public static class GrpcMapper
{
    public static LessonView Map(LessonDbView lesson)
    {
        var lessonView = new LessonView
        {
            Id = lesson.Id.ToString(),
            DisciplineTitle = lesson.Discipline,
            LessonType = lesson.LessonType,
            Start = lesson.Start.ToTimestamp(),
            End = lesson.End.ToTimestamp(),
        };
        if (!lesson.AttendancesExists)
        {
            lessonView.NoAttendance = new NoAttendance();
        }
        else
        {
            lessonView.PresentAttendance = new PresentAttendance
            {
                NumberOfAbsentees = lesson.AbsenteesCount,
                Status = lesson.IsValidated ? LessonApproveStatus.ApprovedByTeacher : LessonApproveStatus.NotApprovedByTeacher
            };
        }
        lessonView.Auditorium.AddRange(lesson.Auditoriums.Select(a => new RtuTc.RtuAttend.Models.Auditorium
        {
            Id = a.Id.ToString(),
            Number = a.Number,
            Building = a.Building,
        }));
        lessonView.Teachers.AddRange(lesson.Teachers.Select(t => new Human
        {
            Id = t.Id.ToString(),
            Firstname = t.Firstname,
            Lastname = t.Lastname,
            Middlename = t.Middlename,
        }));
        lessonView.VisitingLogs.AddRange(lesson.VisitingLogs.Select(vl => new Visitinglog
        {
            VisitingLogId = vl.Id.ToString(),
            Title = vl.Title,
            IsArchived = vl.IsArchieved,
            SemesterId = vl.SemesterId.ToString()
        }));
        return lessonView;
    }
}
