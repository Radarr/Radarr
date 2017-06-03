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

            //Search = new HttpRequestBuilder("https://api.spotify.com/{version}/{route}/") // TODO: maybe use {version} 
            //    .SetSegment("version", "v1")
            //    .CreateFactory();
            Search = new HttpRequestBuilder("http://localhost:5000/{route}/") // TODO: maybe use {version} 
                .CreateFactory();

            InternalSearch = new HttpRequestBuilder("https://itunes.apple.com/WebObjects/MZStore.woa/wa/{route}") //viewArtist or search
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
