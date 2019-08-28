using Radarr.Http;

namespace NzbDrone.Api
{
    public abstract class NzbDroneFeedModule : RadarrModule
    {
        protected NzbDroneFeedModule(string resource)
            : base("/feed/" + resource.Trim('/'))
        {
        }
    }
}
