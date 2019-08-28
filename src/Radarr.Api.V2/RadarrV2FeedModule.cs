using Radarr.Http;

namespace Radarr.Api.V2
{
    public abstract class RadarrV2FeedModule : RadarrModule
    {
        protected RadarrV2FeedModule(string resource)
            : base("/feed/v2/" + resource.Trim('/'))
        {
        }
    }
}
