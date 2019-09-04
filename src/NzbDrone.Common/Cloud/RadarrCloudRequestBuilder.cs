using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface IRadarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory TMDB { get; }
        IHttpRequestBuilderFactory TMDBSingle { get; }
    }

    public class RadarrCloudRequestBuilder : IRadarrCloudRequestBuilder
    {
        public RadarrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("https://radarr.lidarr.audio/v1/")
                .CreateFactory();

            TMDB = new HttpRequestBuilder("https://api.themoviedb.org/3/{route}/{id}{secondaryRoute}")
                .AddQueryParam("api_key", "1a7373301961d03f97f853a876dd1212")
                .CreateFactory();

            TMDBSingle = new HttpRequestBuilder("https://api.themoviedb.org/3/{route}")
                .AddQueryParam("api_key", "1a7373301961d03f97f853a876dd1212")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; private set; }
        public IHttpRequestBuilderFactory TMDB { get; private set; }
        public IHttpRequestBuilderFactory TMDBSingle { get; private set; }
    }
}
