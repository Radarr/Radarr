using Nancy;

namespace Lidarr.Api.V1
{
    public abstract class LidarrV1FeedModule : NancyModule
    {
        protected LidarrV1FeedModule(string resource)
            : base("/feed/v1/" + resource.Trim('/'))
        {
        }
    }
}
