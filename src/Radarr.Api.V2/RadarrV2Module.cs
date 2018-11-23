using Nancy;

namespace Radarr.Api.V2
{
    public abstract class RadarrV2Module : NancyModule
    {
        protected RadarrV2Module(string resource)
            : base("/api/v2/" + resource.Trim('/'))
        {
        }
    }
}
