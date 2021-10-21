using System;
using Microsoft.AspNetCore.Http;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;

namespace Radarr.Http.Middleware
{
    public interface ICacheableSpecification
    {
        bool IsCacheable(HttpContext context);
    }

    public class CacheableSpecification : ICacheableSpecification
    {
        public bool IsCacheable(HttpContext context)
        {
            if (!RuntimeInfo.IsProduction)
            {
                return false;
            }

            if (context.Request.Query.ContainsKey("h"))
            {
                return true;
            }

            if (context.Request.Path.StartsWithSegments("/api", StringComparison.CurrentCultureIgnoreCase))
            {
                if (context.Request.Path.ToString().ContainsIgnoreCase("/MediaCover"))
                {
                    return true;
                }

                return false;
            }

            if (context.Request.Path.StartsWithSegments("/signalr", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            if (context.Request.Path.Value?.EndsWith("/index.js") ?? false)
            {
                return false;
            }

            if (context.Request.Path.Value?.EndsWith("/initialize.js") ?? false)
            {
                return false;
            }

            if (context.Request.Path.StartsWithSegments("/feed", StringComparison.CurrentCultureIgnoreCase))
            {
                return false;
            }

            if (context.Request.Path.StartsWithSegments("/log", StringComparison.CurrentCultureIgnoreCase) &&
                (context.Request.Path.Value?.EndsWith(".txt", StringComparison.CurrentCultureIgnoreCase) ?? false))
            {
                return false;
            }

            if (context.Response != null)
            {
                if (context.Response.ContentType?.Contains("text/html") ?? false || context.Response.StatusCode >= 400)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
