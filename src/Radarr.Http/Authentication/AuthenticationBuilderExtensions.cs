using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using NzbDrone.Core.Authentication;
using NzbDrone.Core.Configuration;

namespace Radarr.Http.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder authenticationBuilder, string name, Action<ApiKeyAuthenticationOptions> options)
        {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(name, options);
        }

        public static AuthenticationBuilder AddBasicAuthentication(this AuthenticationBuilder authenticationBuilder)
        {
            return authenticationBuilder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(AuthenticationType.Basic.ToString(), options => { });
        }

        public static AuthenticationBuilder AddNoAuthentication(this AuthenticationBuilder authenticationBuilder)
        {
            return authenticationBuilder.AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>(AuthenticationType.None.ToString(), options => { });
        }

        public static AuthenticationBuilder AddAppAuthentication(this IServiceCollection services, IConfigFileProvider config)
        {
            var authBuilder = services.AddAuthentication(config.AuthenticationMethod.ToString());

            if (config.AuthenticationMethod == AuthenticationType.Basic)
            {
                authBuilder.AddBasicAuthentication();
            }
            else if (config.AuthenticationMethod == AuthenticationType.Forms)
            {
                authBuilder.AddCookie(AuthenticationType.Forms.ToString(), options =>
                {
                    options.AccessDeniedPath = "/login?loginFailed=true";
                    options.LoginPath = "/login";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });
            }
            else
            {
                authBuilder.AddNoAuthentication();
            }

            authBuilder.AddApiKey("API", options =>
            {
                options.HeaderName = "X-Api-Key";
                options.QueryName = "apikey";
                options.ApiKey = config.ApiKey;
            });

            authBuilder.AddApiKey("SignalR", options =>
            {
                options.HeaderName = "X-Api-Key";
                options.QueryName = "access_token";
                options.ApiKey = config.ApiKey;
            });

            return authBuilder;
        }
    }
}
