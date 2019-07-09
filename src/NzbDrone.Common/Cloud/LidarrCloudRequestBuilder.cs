using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface ILidarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory Search { get; }
        IHttpRequestBuilderFactory InternalSearch { get; }
    }

    public class LidarrCloudRequestBuilder : ILidarrCloudRequestBuilder
    {
        public LidarrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("https://services.lidarr.audio/v1/")
                .CreateFactory();

            Search = new HttpRequestBuilder("https://api.lidarr.audio/api/v0.4/{route}")
                .KeepAlive()
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; }

        public IHttpRequestBuilderFactory Search { get; }

        public IHttpRequestBuilderFactory InternalSearch { get; }
    }
}
