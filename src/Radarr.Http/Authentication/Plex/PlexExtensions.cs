using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Radarr.Http.Authentication.Plex
{
    public static class PlexExtensions
    {
        public static AuthenticationBuilder AddPlex(this AuthenticationBuilder builder, string authenticationScheme, Action<PlexOptions> configureOptions)
            => builder.AddOAuth<PlexOptions, PlexHandler>(authenticationScheme, PlexDefaults.DisplayName, configureOptions);
    }
}
