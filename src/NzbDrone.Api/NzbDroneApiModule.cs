using Radarr.Http;

namespace NzbDrone.Api
{
    public abstract class NzbDroneApiModule : RadarrModule
    {
        protected NzbDroneApiModule(string resource)
            : base("/api/" + resource.Trim('/'))
        {
        }
    }
}
