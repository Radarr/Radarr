using Radarr.Http;

namespace Radarr.Api.V3
{
    public abstract class RadarrV3FeedModule : RadarrModule
    {
        protected RadarrV3FeedModule(string resource)
            : base("/feed/v3/" + resource.Trim('/'))
        {
        }
    }
}
