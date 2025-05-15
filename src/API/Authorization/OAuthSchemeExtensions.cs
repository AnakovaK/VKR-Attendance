using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using RTUAttendAPI.API.Authorization.MIREA;

namespace RTUAttendAPI.API.Authorization;

public static class OAuthSchemeExtensions
{
    public static AuthenticationBuilder AddMireaOauth(
        this AuthenticationBuilder authenticationBuilder,
        IConfigurationSection oauthSection)
    {
        authenticationBuilder.AddOAuth<MIREAOptions, MIREAHandler>(MIREADefaults.AuthenticationScheme, MIREADefaults.DisplayName, oauthSection.Bind);
        authenticationBuilder.Services.AddSingleton<MIREAStateDateFormatInCache>();
        authenticationBuilder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<MIREAOptions>, CacheStateDateFormatInjector>());
        return authenticationBuilder;
    }
    private class CacheStateDateFormatInjector : IPostConfigureOptions<MIREAOptions>
    {
        private readonly MIREAStateDateFormatInCache _stateDateFormat;

        public CacheStateDateFormatInjector(MIREAStateDateFormatInCache stateDateFormat)
        {
            this._stateDateFormat = stateDateFormat;
        }
        public void PostConfigure(string? name, MIREAOptions options) 
            => options.StateDataFormat = _stateDateFormat;
    }
}
