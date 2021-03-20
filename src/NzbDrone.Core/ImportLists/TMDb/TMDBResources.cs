using Newtonsoft.Json;

namespace NzbDrone.Core.ImportLists.TMDb
{
    public class MovieSearchResource
    {
        public int Page { get; set; }
        public MovieResultResource[] Results { get; set; }

        [JsonProperty("total_results")]
        public int TotalResults { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }
    }

    public class AuthRefreshTokenResource
    {
        [JsonProperty("request_token")]
        public string RequestToken { get; set; }
    }

    public class AuthAccessTokenResource
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("account_id")]
        public string AccountId { get; set; }
    }

    public class MovieResultResource
    {
        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }
        public bool Adult { get; set; }
        public string Overview { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("genre_ids")]
        public int?[] GenreIds { get; set; }
        public int Id { get; set; }

        [JsonProperty("original_title")]
        public string OriginalTitle { get; set; }

        [JsonProperty("original_language")]
        public string OriginalLanguage { get; set; }
        public string Title { get; set; }

        [JsonProperty("backdrop_path")]
        public string BackdropPath { get; set; }
        public float Popularity { get; set; }

        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }
        public bool Video { get; set; }

        [JsonProperty("vote_average")]
        public float VoteAverage { get; set; }

        [JsonProperty("trailer_key")]
        public string TrailerKey { get; set; }

        [JsonProperty("trailer_site")]
        public string TrailerSite { get; set; }

        [JsonProperty("physical_release")]
        public string PhysicalRelease { get; set; }

        [JsonProperty("physical_release_note")]
        public string PhysicalReleaseNote { get; set; }
    }

    public class CreditsResultResource : MovieResultResource
    {
        public string Department { get; set; }
        public string Job { get; set; }

        [JsonProperty("credit_id")]
        public string CreditId { get; set; }
    }

    public class ListResponseResource
    {
        public string Id { get; set; }
        public ListItemResource[] Results { get; set; }

        [JsonProperty("total_results")]
        public int TotalResults { get; set; }

        [JsonProperty("total_pages")]
        public int TotalPages { get; set; }

        [JsonProperty("iso_639_1")]
        public string Iso639 { get; set; }
        public string Name { get; set; }

        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }
    }

    public class CollectionResponseResource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }

        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }

        [JsonProperty("backdrop_path")]
        public string BackdropPath { get; set; }
        public MovieResultResource[] Parts { get; set; }
    }

    public class PersonCreditsResource
    {
        public CreditsResultResource[] Cast { get; set; }
        public CreditsResultResource[] Crew { get; set; }
        public int Id { get; set; }
    }

    public class ListItemResource : MovieResultResource
    {
        [JsonProperty("media_type")]
        public string MediaType { get; set; }
        [JsonProperty("origin_country")]
        public string[] OriginCountry { get; set; }
        public string Name { get; set; }
        [JsonProperty("original_name")]
        public string OriginalName { get; set; }
    }
}
