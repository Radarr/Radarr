using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface ISonarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory SkyHookTvdb { get; }
        IHttpRequestBuilderFactory TMDB { get; }
        IHttpRequestBuilderFactory TMDBSingle { get; }
    }

    public class SonarrCloudRequestBuilder : ISonarrCloudRequestBuilder
    {
        public SonarrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("http://radarr.aeonlucid.com/v1/")
                .CreateFactory();

            SkyHookTvdb = new HttpRequestBuilder("http://skyhook.sonarr.tv/v1/tvdb/{route}/{language}/")
                .SetSegment("language", "en")
                .CreateFactory();

            TMDB = new HttpRequestBuilder("https://api.themoviedb.org/3/{route}/{id}{secondaryRoute}")
                .AddQueryParam("api_key", "1a7373301961d03f97f853a876dd1212")
                .CreateFactory();

            TMDBSingle = new HttpRequestBuilder("https://api.themoviedb.org/3/{route}")
                .AddQueryParam("api_key", "1a7373301961d03f97f853a876dd1212")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; private set; }
        public IHttpRequestBuilderFactory SkyHookTvdb { get; private set; }
        public IHttpRequestBuilderFactory TMDB { get; private set; }
        public IHttpRequestBuilderFactory TMDBSingle { get; private set; }
    }
}
