using NzbDrone.Common.Http;

namespace NzbDrone.Common.Cloud
{
    public interface IRadarrCloudRequestBuilder
    {
        IHttpRequestBuilderFactory Services { get; }
        IHttpRequestBuilderFactory TMDB { get; }
        IHttpRequestBuilderFactory RadarrMetadata { get; }
    }

    public class RadarrCloudRequestBuilder : IRadarrCloudRequestBuilder
    {
        public RadarrCloudRequestBuilder()
        {
            Services = new HttpRequestBuilder("https://radarr.servarr.com/v1/")
                .CreateFactory();

            TMDB = new HttpRequestBuilder("https://api.themoviedb.org/{api}/{route}/{id}{secondaryRoute}")
                .SetHeader("Authorization", $"Bearer {AuthToken}")
                .CreateFactory();

            RadarrMetadata = new HttpRequestBuilder("https://api.radarr.video/v1/{route}")
                .CreateFactory();
        }

        public IHttpRequestBuilderFactory Services { get; private set; }
        public IHttpRequestBuilderFactory TMDB { get; private set; }
        public IHttpRequestBuilderFactory RadarrMetadata { get; private set; }

        public string AuthToken => "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIxYTczNzMzMDE5NjFkMDNmOTdmODUzYTg3NmRkMTIxMiIsInN1YiI6IjU4NjRmNTkyYzNhMzY4MGFiNjAxNzUzNCIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.gh1BwogCCKOda6xj9FRMgAAj_RYKMMPC3oNlcBtlmwk";
    }
}
