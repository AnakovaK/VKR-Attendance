using System.Diagnostics;

using Ardalis.Specification;

using AttendDatabase;

using Quartz;

using RTUAttendAPI.API.Extensions;
using RTUAttendAPI.AttendDatabase.Models;
using RTUAttendAPI.AttendDatabase.Specs;

namespace RTUAttendAPI.API.Services.BackgroundSchedule;

[DisallowConcurrentExecution]
internal class BackgroundGroupActualizeJob : IJob, IDisposable
{
    private readonly IRepositoryBase<VisitingLog> _visitingLogRep;
    private readonly IRepositoryBase<NsiStudent> _nsiStudentRep;
    private readonly IRepositoryBase<NsiHuman> _nsiHumanRep;
    private readonly IUniversityDataProvider _universityDataProvider;
    private readonly AttendDbContext _attendDbContext;
    private readonly ILogger<BackgroundGroupActualizeJob> _logger;

    public BackgroundGroupActualizeJob(
        IRepositoryBase<VisitingLog> visitingLogRep,
        IRepositoryBase<NsiStudent> nsiStudentRep,
        IRepositoryBase<NsiHuman> nsiHumanRep,
        IUniversityDataProvider universityDataProvider,
        AttendDbContext attendDbContext, // используется для обновления TODO: разнести выборки и изменения в разные grpc сервисы (мини CQRS)
        ILogger<BackgroundGroupActualizeJob> logger)
    {
        _visitingLogRep = visitingLogRep;
        _nsiStudentRep = nsiStudentRep;
        _nsiHumanRep = nsiHumanRep;
        _universityDataProvider = universityDataProvider;
        _attendDbContext = attendDbContext;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        
        JobDataMap dataMap = context.MergedJobDataMap;  // Получение названия группы и семестра из job
        var groupTitle = dataMap.GetString("givenGroup")!;
        var semesterId = dataMap.GetString("currentSemester")!;

        var currentVisitingLog = await _visitingLogRep.SingleOrDefaultAsync(new GroupCompositionForTandemSpec(groupTitle, semesterId.AsGuid()), context.CancellationToken);
        if (currentVisitingLog == null)
        {
            throw new UnreachableException($"Job must be scheduled only for existing visiting logs {groupTitle}, {semesterId}"); //TODO: вынести Include в record
        }
        if (currentVisitingLog.Source == VisitingLogSource.Manual)
        {
            _logger.LogInformation("Testing manual visiting log, no changes to memberships applied");
            return;
        }
        var actualizedVisitingLog = await _universityDataProvider.GetAcademicGroupComposition(groupTitle);
        var actualizedVisitingLogElderId = actualizedVisitingLog.ElderId;

        // Если не находится в нашей БД => это новичок
        foreach (var student in actualizedVisitingLog.NsiStudents)
        {
            var correctStudentRole = student.Id == actualizedVisitingLogElderId ? StudentMembershipRole.Elder : StudentMembershipRole.Student;

            if (student.PersonalNumber == "20И1302" || student.PersonalNumber == "21И1138")
            {
                correctStudentRole = StudentMembershipRole.Elder;
            }

            var targetMembership = currentVisitingLog.StudentMemberships.SingleOrDefault(m => m.NsiStudentId == student.Id);

            if (targetMembership is null)
            {
                var newStudentMembership = new StudentMembership
                {
                    NsiStudentId = student.Id,
                    VisitingLogId = currentVisitingLog.Id,
                    MembershipType = StudentMembershipType.Active,
                    MembershipRole = correctStudentRole
                };
                _attendDbContext.StudentMemberships.Add(newStudentMembership);
                _logger.LogDebug("New student was added by id: {NewStudent} in visiting log {CurrentVisitingLog}", newStudentMembership.NsiStudentId, currentVisitingLog.Id);
            }
            else if (targetMembership.MembershipRole != correctStudentRole)
            {
                var updatedStudent = new StudentMembership
                {
                    Id = targetMembership.Id,
                    NsiStudentId = targetMembership.NsiStudentId,
                    VisitingLogId = targetMembership.VisitingLogId,
                    MembershipType = targetMembership.MembershipType,
                    MembershipRole = correctStudentRole,
                };
                _attendDbContext.StudentMemberships.Update(updatedStudent);
                _logger.LogInformation("Student {updatedStudent} status was changed to inactive", updatedStudent.NsiStudentId);
            }
        }
        // Если не находится actualized group => Уволен из группы
        foreach (var inactiveStudentMembership in currentVisitingLog.StudentMemberships
            .Where(s => s.MembershipType != StudentMembershipType.Inactive)
            .ExceptBy(actualizedVisitingLog.NsiStudents.Select(s => s.Id), s => s.NsiStudentId))
        {
            var updatedStudent = new StudentMembership
            {
                Id = inactiveStudentMembership.Id,
                NsiStudentId = inactiveStudentMembership.NsiStudentId,
                VisitingLogId = inactiveStudentMembership.VisitingLogId,
                MembershipType = StudentMembershipType.Inactive,
                MembershipRole = inactiveStudentMembership.MembershipRole,
            };
            _attendDbContext.StudentMemberships.Update(updatedStudent);
        }
        try
        {
            await _attendDbContext.SaveChangesAsync(context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't actualize group {GroupTitle}", groupTitle);
        }
    }


    public void Dispose()
    {
        _logger.LogInformation("Background actualized group job disposing");
    }
}
