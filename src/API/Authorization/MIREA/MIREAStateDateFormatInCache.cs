using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;

namespace RTUAttendAPI.API.Authorization.MIREA;

/// <summary>
/// Кастомная хранилка состояния авторизации, так как стейт по умолчанию слишком большой для МИРЭА
/// </summary>
public class MIREAStateDateFormatInCache : ISecureDataFormat<AuthenticationProperties>
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<MIREAStateDateFormatInCache> _logger;

    public MIREAStateDateFormatInCache(IDistributedCache cache, ILogger<MIREAStateDateFormatInCache> logger)
    {
        this._cache = cache;
        this._logger = logger;
    }
    public string Protect(AuthenticationProperties data)
        => Protect(data, null);

    public string Protect(AuthenticationProperties data, string? purpose)
    {
        var key = $"mirea-state-{Guid.NewGuid()}";
        var stringValue = JsonSerializer.Serialize(new AuthenticationPropertiesWithPurpose(data, purpose));
        _cache.SetString(key, stringValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });
        return key;
    }

    public AuthenticationProperties? Unprotect(string? protectedText)
        => Unprotect(protectedText, null);

    public AuthenticationProperties? Unprotect(string? protectedText, string? purpose)
    {
        if (string.IsNullOrEmpty(protectedText))
        {
            return default;
        }
        try
        {
            var cachedValue = _cache.GetString(protectedText);
            if (string.IsNullOrEmpty(cachedValue))
            {
                return default;
            }
            var parsed = JsonSerializer.Deserialize<AuthenticationPropertiesWithPurpose>(cachedValue);
            if (parsed is not null && parsed.Purpose == purpose)
            {
                _cache.Remove(protectedText);
                return parsed.Props;
            }
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Can't handle mirea state");
            return default;
        }
    }
    private record AuthenticationPropertiesWithPurpose(AuthenticationProperties Props, string? Purpose);
}
