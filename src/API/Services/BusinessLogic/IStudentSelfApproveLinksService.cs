using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

using RTUAttendAPI.API.Configuration;
using RTUAttendAPI.AttendDatabase.Models;

namespace RTUAttendAPI.API.Services.BusinessLogic;

public interface IStudentSelfApproveLinksService
{
    /// <summary>
    /// Создает временную ссылку для самостоятельного подтверждения студентов на занятии
    /// </summary>
    /// <param name="lessonId">id занятия</param>
    /// <returns>временная ссылка для подтверждения</returns>
    Task<string> CreateLinkForLesson(Guid lessonId, CancellationToken cancellationToken);
    /// <summary>
    /// Ожидает время, необходимое для генерации новой ссылки
    /// </summary>
    /// <returns></returns>
    Task WaitForNextLink(CancellationToken cancellationToken);
    /// <summary>
    /// Выдает идентификатор занятия, для которого был выпущен токен, если он еще валиден
    /// </summary>
    /// <param name="token"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Guid?> GetLessonIdForToken(string token, CancellationToken cancellationToken);
}

public class DistributedCacheApproveService : IStudentSelfApproveLinksService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IOptionsSnapshot<StudentSelfApproveLinkOptions> _options;

    public DistributedCacheApproveService(IDistributedCache distributedCache, IOptionsSnapshot<StudentSelfApproveLinkOptions> options)
    {
        _distributedCache = distributedCache;
        _options = options;
    }
    public async Task<string> CreateLinkForLesson(Guid lessonId, CancellationToken cancellationToken)
    {
        var token = Guid.NewGuid().ToString();
        var link = string.Format(_options.Value.LinkTemplate, token);
        await _distributedCache.SetStringAsync(token, lessonId.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = _options.Value.LinkLifetime
        }, cancellationToken);
        return link;
    }

    public Task WaitForNextLink(CancellationToken cancellationToken) 
        => Task.Delay(_options.Value.DelayBetweenCreatingLinks, cancellationToken);
    
    public async Task<Guid?> GetLessonIdForToken(string token, CancellationToken cancellationToken)
    {
        var lessonIdString = await _distributedCache.GetStringAsync(token, cancellationToken);
        return lessonIdString == null ? null : Guid.TryParse(lessonIdString, out var lessonId) ? lessonId : null;
    }
}
