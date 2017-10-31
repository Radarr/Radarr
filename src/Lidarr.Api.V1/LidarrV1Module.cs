using Nancy;

namespace Lidarr.Api.V1
{
    public abstract class LidarrV1Module : NancyModule
    {
        protected LidarrV1Module(string resource)
            : base("/api/v1/" + resource.Trim('/'))
        {
        }
    }
}
