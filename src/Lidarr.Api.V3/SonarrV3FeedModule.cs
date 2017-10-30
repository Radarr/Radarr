using Nancy;

namespace Lidarr.Api.V3
{
    public abstract class SonarrV3FeedModule : NancyModule
    {
        protected SonarrV3FeedModule(string resource)
            : base("/feed/v3/" + resource.Trim('/'))
        {
        }
    }
}