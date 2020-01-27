using Radarr.Http;

namespace Radarr.Api.V3
{
    public abstract class RadarrV3Module : RadarrModule
    {
        protected RadarrV3Module(string resource)
            : base("/api/v3/" + resource.Trim('/'))
        {
        }
    }
}
