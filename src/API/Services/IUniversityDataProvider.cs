using RtuTc.TandemSchedule;

namespace RTUAttendAPI.API.Services;

public interface IUniversityDataProvider
{
    Task<GetAcademicGroupCompositionResponse> GetAcademicGroupComposition(string groupTitle);
}

internal class TandemGrpcProvider : IUniversityDataProvider
{
    private readonly TandemSchedule.TandemScheduleClient _tandemScheduleClient;
    private readonly ILogger<TandemGrpcProvider> _logger;

    public TandemGrpcProvider(
        TandemSchedule.TandemScheduleClient tandemScheduleClient,
        ILogger<TandemGrpcProvider> logger)
    {
        _tandemScheduleClient = tandemScheduleClient;
        _logger = logger;
    }

    public async Task<GetAcademicGroupCompositionResponse> GetAcademicGroupComposition(string groupTitle)
    {
        var request = new GetAcademicGroupCompositionRequest
        {
            GroupTitle = groupTitle
        };
        return await _tandemScheduleClient.GetAcademicGroupCompositionAsync(request);
    }
}

internal class StaticDataProvider : IUniversityDataProvider
{
    public Task<GetAcademicGroupCompositionResponse> GetAcademicGroupComposition(string groupTitle)
    {
        var response = new GetAcademicGroupCompositionResponse
        {
            ElderId = "write youself"
        };

        response.NsiStudents.Add(new NsiStudent
        {
            Human = new NsiHuman
            {
                LastName = "Ivanov",
                FirstName = "Ivan",
                MiddleName = "Ivanovich",
                NsiHumanId = "A4E61FBF-2DF8-4D8B-8599-D89EBEA80D4C"
            },
            NsiStudentId = "A5E61FBF-2DF8-4D8B-8599-D89EBEA80D4C",
            PersonalNumber = "I56B1234"
        });


        return Task.FromResult(response);
    }
}
