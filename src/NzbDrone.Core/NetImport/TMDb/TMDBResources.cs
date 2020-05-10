namespace NzbDrone.Core.NetImport.TMDb
{
    public class MovieSearchRoot
    {
        public int Page { get; set; }
        public MovieResult[] Results { get; set; }
        public int total_results { get; set; }
        public int total_pages { get; set; }
    }

    public class AuthRefreshTokenResponse
    {
        public string request_token { get; set; }
    }

    public class AuthAccessTokenResponse
    {
        public string access_token { get; set; }
        public string account_id { get; set; }
    }

    public class MovieResult
    {
        public string poster_path { get; set; }
        public bool adult { get; set; }
        public string overview { get; set; }
        public string release_date { get; set; }
        public int?[] genre_ids { get; set; }
        public int id { get; set; }
        public string original_title { get; set; }
        public string original_language { get; set; }
        public string title { get; set; }
        public string backdrop_path { get; set; }
        public float popularity { get; set; }
        public int vote_count { get; set; }
        public bool video { get; set; }
        public float vote_average { get; set; }
        public string trailer_key { get; set; }
        public string trailer_site { get; set; }
        public string physical_release { get; set; }
        public string physical_release_note { get; set; }
    }

    public class CreditsResult : MovieResult
    {
        public string department { get; set; }
        public string job { get; set; }
        public string credit_id { get; set; }
    }

    public class ListResponseRoot
    {
        public string id { get; set; }
        public Item[] items { get; set; }
        public int item_count { get; set; }
        public string iso_639_1 { get; set; }
        public string name { get; set; }
        public object poster_path { get; set; }
    }

    public class CollectionResponseRoot
    {
        public int id { get; set; }
        public string name { get; set; }
        public string overview { get; set; }
        public string poster_path { get; set; }
        public string backdrop_path { get; set; }
        public MovieResult[] parts { get; set; }
    }

    public class PersonCreditsRoot
    {
        public CreditsResult[] cast { get; set; }
        public CreditsResult[] crew { get; set; }
        public int id { get; set; }
    }

    public class Item : MovieResult
    {
        public string media_type { get; set; }
        public string first_air_date { get; set; }
        public string[] origin_country { get; set; }
        public string name { get; set; }
        public string original_name { get; set; }
    }
}
