using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface ILidarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory Search { get; }
        IHttpRequestBuilderFactory InternalSearch { get; }
        IHttpRequestBuilderFactory SkyHookTvdb { get; }
    }

    public class LidarrCloudRequestBuilder : ILidarrCloudRequestBuilder
    {
        public LidarrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("http://services.lidarr.tv/v1/")
                .CreateFactory();

            Search = new HttpRequestBuilder("http://localhost:5000/{route}/") // TODO: Add {version} once LidarrAPI.Metadata is released. 
                .CreateFactory();


            SkyHookTvdb = new HttpRequestBuilder("http://skyhook.lidarr.tv/v1/tvdb/{route}/{language}/")
                .SetSegment("language", "en")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; }

        public IHttpRequestBuilderFactory Search { get; }

        public IHttpRequestBuilderFactory InternalSearch { get; }

        public IHttpRequestBuilderFactory SkyHookTvdb { get; }
    }
}
