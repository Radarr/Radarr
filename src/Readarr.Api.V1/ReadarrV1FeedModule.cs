using Readarr.Http;

namespace Readarr.Api.V1
{
    public abstract class ReadarrV1FeedModule : ReadarrModule
    {
        protected ReadarrV1FeedModule(string resource)
            : base("/feed/v1/" + resource.Trim('/'))
        {
        }
    }
}
