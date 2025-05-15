using RTUAttendAPI.AttendDatabase.Models;

namespace RTUAttendAPI.API.Services;

public interface IUniversityDataProvider
{
    Task<GetAcademicGroupCompositionResponse> GetAcademicGroupComposition(string groupTitle);
}

internal class TandemGrpcProvider : IUniversityDataProvider
{
    private readonly ILogger<TandemGrpcProvider> _logger;

    public TandemGrpcProvider(
        ILogger<TandemGrpcProvider> logger)
    {
        _logger = logger;
    }

    public async Task<GetAcademicGroupCompositionResponse> GetAcademicGroupComposition(string groupTitle)
    {
        var response = new GetAcademicGroupCompositionResponse
        {
            ElderId = new Guid("A5E61FBF-2DF8-4D8B-8599-D89EBEA80D4C"),
        };

        response.NsiStudents.Add(new NsiStudent
        {
            NsiHuman = new NsiHuman
            {
                Lastname = "Ivanov",
                Firstname = "Ivan",
                Middlename = "Ivanovich",
                Id = new Guid("A4E61FBF-2DF8-4D8B-8599-D89EBEA80D4C")
            },
            Id = new Guid("A5E61FBF-2DF8-4D8B-8599-D89EBEA80D4C"),
            PersonalNumber = "I56B1234"
        });

        return response;
    }
}

internal class StaticDataProvider : IUniversityDataProvider
{
    public Task<GetAcademicGroupCompositionResponse> GetAcademicGroupComposition(string groupTitle)
    {
        var response = new GetAcademicGroupCompositionResponse
        {
            ElderId = new Guid("A5E61FBF-2DF8-4D8B-8599-D89EBEA80D4C"),
        };

        response.NsiStudents.Add(new NsiStudent
        {
            NsiHuman = new NsiHuman
            {
                Lastname = "Ivanov",
                Firstname = "Ivan",
                Middlename = "Ivanovich",
                Id = new Guid("A4E61FBF-2DF8-4D8B-8599-D89EBEA80D4C")
            },
            Id = new Guid("A5E61FBF-2DF8-4D8B-8599-D89EBEA80D4C"),
            PersonalNumber = "I56B1234"
        });

        return Task.FromResult(response);
    }
}

public class GetAcademicGroupCompositionResponse
{
    public List<NsiStudent> NsiStudents { get; set; } = new List<NsiStudent>();
    public Guid ElderId { get; set; }
}
