using Ardalis.Specification.EntityFrameworkCore;

using AttendDatabase;

using Grpc.Core;

using Microsoft.EntityFrameworkCore;

using RTUAttendAPI.API.Extensions;
using RTUAttendAPI.AttendDatabase.Models;
using RTUAttendAPI.AttendDatabase.Specs;


namespace RTUAttendAPI.API.Services.Initialization;

public interface IHumanLoginInfoService
{
    Task<NsiHuman> GetHumanLoginInfoByLogin(Guid loginId, CancellationToken cancellationToken);
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

    public ReadFromDbOrCreateFromTandemHumanService(AttendDbContext attendDbContext)
    {
        _attendDbContext = attendDbContext;
    }
    public async Task<NsiHuman> GetHumanLoginInfoByLogin(Guid loginId, CancellationToken cancellationToken)
    {
        var humanInfo = await _attendDbContext.NsiHumans
            .WithSpecification(new GetHumanLoginInfoByLoginIdSpec(loginId))
            .SingleOrDefaultAsync(cancellationToken);
        if (humanInfo != default)
        {
            return humanInfo;
        }
        var human = new AttendDatabase.Models.NsiHuman
        {
            Id = humanInfo.Id,
            Firstname = humanInfo.Firstname,
            Lastname = humanInfo.Lastname,
            Middlename = humanInfo.Middlename,
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
        await _attendDbContext.SaveChangesAsync();
        return human;
    }
}


