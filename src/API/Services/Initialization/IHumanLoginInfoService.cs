using Ardalis.Specification.EntityFrameworkCore;

using AttendDatabase;

using Grpc.Core;

using Microsoft.EntityFrameworkCore;

using RTUAttendAPI.API.Extensions;
using RTUAttendAPI.AttendDatabase.Models;
using RTUAttendAPI.AttendDatabase.Specs;

using RtuTc.TandemSchedule;

namespace RTUAttendAPI.API.Services.Initialization;

public interface IHumanLoginInfoService
{
    Task<HumanLoginInfo> GetHumanLoginInfoByLogin(Guid loginId, CancellationToken cancellationToken);
}

public class CantGetUserInfoException : Exception
{
    public Guid LoginId { get; set; }
    public CantGetUserInfoException(Guid loginId) : base($"Can't find info about login id {loginId}")
    {
        LoginId = loginId;
    }
}

public class ReadFromDbOrCreateFromTandemHumanService : IHumanLoginInfoService
{
    private readonly AttendDbContext _attendDbContext;
    private readonly TandemSchedule.TandemScheduleClient _tandem;

    public ReadFromDbOrCreateFromTandemHumanService(AttendDbContext attendDbContext, TandemSchedule.TandemScheduleClient tandem)
    {
        _attendDbContext = attendDbContext;
        _tandem = tandem;
    }
    public async Task<HumanLoginInfo> GetHumanLoginInfoByLogin(Guid loginId, CancellationToken cancellationToken)
    {
        var humanIdFromDb = await _attendDbContext.NsiHumans
            .WithSpecification(new GetHumanLoginInfoByLoginIdSpec(loginId))
            .SingleOrDefaultAsync(cancellationToken);
        if (humanIdFromDb != default)
        {
            return humanIdFromDb;
        }
        var actualTandemInfo = await _tandem.GetInfoAboutHumanByLoginIdAsync(new GetInfoAboutHumanByLoginIdRequest
        {
            LoginId = loginId.ToString()
        }, new CallOptions(cancellationToken: cancellationToken));
        if (actualTandemInfo.ScheduleCase != GetInfoAboutHumanByLoginIdResponse.ScheduleOneofCase.Found)
        {
            throw new CantGetUserInfoException(loginId);
        }
        var humanInfo = actualTandemInfo.Found.Human;
        var human = new AttendDatabase.Models.NsiHuman
        {
            Id = humanInfo.NsiHumanId.AsGuid(),
            Firstname = humanInfo.FirstName,
            Lastname = humanInfo.LastName,
            Middlename = humanInfo.MiddleName,
        };
        _attendDbContext.NsiHumans.Add(human);
        if (human.Id != loginId)
        {
            var login = new NsiHumanLoginId
            {
                HumanId = human.Id,
                LoginId = loginId,
            };
            _attendDbContext.NsiHumanLoginIds.Add(login);
        }
        if (actualTandemInfo.Found.Teacher is not null)
        {
            var teacher = new AttendDatabase.Models.Teacher
            {
                NsiHumanId = human.Id,
            };
            _attendDbContext.Teachers.Add(teacher);
        }
        _attendDbContext.NsiStudents.AddRange(actualTandemInfo.Found.Students.Select(s => new AttendDatabase.Models.NsiStudent
        {
            NsiHumanId = human.Id,
            PersonalNumber = s.PersonalNumber,
        }));
        await _attendDbContext.SaveChangesAsync();
        return new HumanLoginInfo(human.Id, actualTandemInfo.Found.Teacher is not null);
    }
}


