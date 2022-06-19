using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Authentication;
using Radarr.Http.Authentication.Plex;

namespace Radarr.Http.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder authenticationBuilder, string name, Action<ApiKeyAuthenticationOptions> options)
        {
            return authenticationBuilder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(name, options);
        }

        public static AuthenticationBuilder AddBasic(this AuthenticationBuilder authenticationBuilder, string name)
        {
            return authenticationBuilder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>(name, options => { });
        }

        public static AuthenticationBuilder AddNone(this AuthenticationBuilder authenticationBuilder, string name)
        {
            return authenticationBuilder.AddScheme<AuthenticationSchemeOptions, NoAuthenticationHandler>(name, options => { });
        }

        public static string GetChallengeScheme(this AuthenticationType scheme)
        {
            return scheme.ToString() + "Remote";
        }

        public static AuthenticationBuilder AddAppAuthentication(this IServiceCollection services)
        {
            var builder = services.AddAuthentication()
                .AddNone(AuthenticationType.None.ToString())
                .AddNone(AuthenticationType.External.ToString())
                .AddBasic(AuthenticationType.Basic.ToString())
                .AddCookie(AuthenticationType.Forms.ToString(), options =>
                {
                    options.Cookie.Name = BuildInfo.AppName + "Auth";
                    options.LoginPath = "/login";
                    options.AccessDeniedPath = "/login/failed";
                    options.LogoutPath = "/logout";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                })
                .AddCookie(AuthenticationType.Plex.ToString(), options =>
                {
                    options.Cookie.Name = BuildInfo.AppName + "PlexAuth";
                    options.LoginPath = "/login/sso";
                    options.AccessDeniedPath = "/login/sso/failed";
                    options.LogoutPath = "/logout";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                })
                .AddPlex(AuthenticationType.Plex.GetChallengeScheme(), options =>
                {
                    options.SignInScheme = AuthenticationType.Plex.ToString();
                    options.CorrelationCookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                })
                .AddCookie(AuthenticationType.Oidc.ToString(), options =>
                {
                    options.Cookie.Name = BuildInfo.AppName + "OidcAuth";
                    options.LoginPath = "/login/sso";
                    options.AccessDeniedPath = "/login/sso/failed";
                    options.LogoutPath = "/logout";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                })
                .AddOpenIdConnect(AuthenticationType.Oidc.GetChallengeScheme(), _ => { } /* See ConfigureOidcOptions.cs */)
                .AddApiKey("API", options =>
                {
                    options.HeaderName = "X-Api-Key";
                    options.QueryName = "apikey";
                })
                .AddApiKey("SignalR", options =>
                {
                    options.HeaderName = "X-Api-Key";
                    options.QueryName = "access_token";
                });

            return builder;
        }
    }
}
