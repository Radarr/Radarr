using Lidarr.Http;

namespace Lidarr.Api.V1
{
    public abstract class LidarrV1Module : LidarrModule
    {
        protected LidarrV1Module(string resource)
            : base("/api/v1/" + resource.Trim('/'))
        {
        }
    }
}
