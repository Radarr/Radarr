using Radarr.Http;

namespace Radarr.Api.V2
{
    public abstract class RadarrV2Module : RadarrModule
    {
        protected RadarrV2Module(string resource)
            : base("/api/v2/" + resource.Trim('/'))
        {
        }
    }
}
