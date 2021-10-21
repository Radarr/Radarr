using System;
using Microsoft.AspNetCore.Http;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Radarr.Http.Middleware
{
    public interface ICacheableSpecification
    {
        bool IsCacheable(HttpRequest request);
    }

    public class CacheableSpecification : ICacheableSpecification
    {
        public bool IsCacheable(HttpRequest request)
        {
            if (!RuntimeInfo.IsProduction)
            {
                return false;
            }

            if (request.Query.ContainsKey("h"))
            {
                return true;
            }

            if (request.Path.StartsWithSegments("/api", StringComparison.CurrentCultureIgnoreCase))
            {
                if (request.Path.ToString().ContainsIgnoreCase("/MediaCover"))
                {
                    return true;
                }

                return false;
            }

            if (request.Path.StartsWithSegments("/signalr", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            if (request.Path.Value?.EndsWith("/index.js") ?? false)
            {
                return false;
            }

            if (request.Path.Value?.EndsWith("/initialize.js") ?? false)
            {
                return false;
            }

            if (request.Path.StartsWithSegments("/feed", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            if (request.Path.StartsWithSegments("/log", StringComparison.CurrentCultureIgnoreCase) &&
                (request.Path.Value?.EndsWith(".txt", StringComparison.CurrentCultureIgnoreCase) ?? false))
            {
                return false;
            }

            return true;
        }
    }
}
