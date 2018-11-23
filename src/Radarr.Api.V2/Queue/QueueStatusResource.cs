using Radarr.Http.REST;

namespace Radarr.Api.V2.Queue
{
    public class QueueStatusResource : RestResource
    {
        public int Count { get; set; }
        public bool Errors { get; set; }
        public bool Warnings { get; set; }
    }
}
