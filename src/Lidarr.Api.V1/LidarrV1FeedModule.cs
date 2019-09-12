using Lidarr.Http;

namespace Lidarr.Api.V1
{
    public abstract class LidarrV1FeedModule : LidarrModule
    {
        protected LidarrV1FeedModule(string resource)
            : base("/feed/v1/" + resource.Trim('/'))
        {
        }
    }
}
