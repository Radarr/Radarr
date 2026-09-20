using Newtonsoft.Json;

namespace NzbDrone.Core.MetadataSource.TMDb
{
    public class TmdbMovieImagesResource
    {
        public int Id { get; set; }
        public TmdbLogoResource[] Logos { get; set; }
    }

    public class TmdbLogoResource
    {
        [JsonProperty("file_path")]
        public string FilePath { get; set; }

        [JsonProperty("vote_average")]
        public double VoteAverage { get; set; }

        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }

        [JsonProperty("iso_639_1")]
        public string Iso6391 { get; set; }
    }
}
