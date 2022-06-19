using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Authentication.OpenIdConnect
{
    public class ConfigureOidcOptions : IConfigureNamedOptions<OpenIdConnectOptions>
    {
        private readonly IConfigService _configService;

        public ConfigureOidcOptions(IConfigService configService)
        {
            _configService = configService;
        }

        public void Configure(string name, OpenIdConnectOptions options)
        {
            options.ClientId = _configService.OidcClientId.IsNullOrWhiteSpace() ? "dummy" : _configService.OidcClientId;
            options.ClientSecret = _configService.OidcClientSecret.IsNullOrWhiteSpace() ? "dummy" : _configService.OidcClientSecret;
            options.Authority = _configService.OidcAuthority.IsNullOrWhiteSpace() ? "https://dummy.com" : _configService.OidcAuthority;
            options.SignedOutRedirectUri = "/login/sso";
            options.SignInScheme = AuthenticationType.Oidc.ToString();
            options.NonceCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
            options.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        }

        public void Configure(OpenIdConnectOptions options)
        => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
