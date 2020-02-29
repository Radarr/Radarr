using Readarr.Http;

namespace Readarr.Api.V1
{
    public abstract class ReadarrV1Module : ReadarrModule
    {
        protected ReadarrV1Module(string resource)
            : base("/api/v1/" + resource.Trim('/'))
        {
        }
    }
}
