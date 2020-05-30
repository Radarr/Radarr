using System.Collections.Generic;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktMovieIdsResource
    {
        public int Trakt { get; set; }
        public string Slug { get; set; }
        public string Imdb { get; set; }
        public int Tmdb { get; set; }
    }

    public class TraktMovieResource
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public TraktMovieIdsResource Ids { get; set; }
    }

    public class TraktResponse
    {
        public int? Rank { get; set; }
        public string Listed_at { get; set; }
        public string Type { get; set; }

        public int? Watchers { get; set; }

        public long? Revenue { get; set; }

        public long? Watcher_count { get; set; }
        public long? Play_count { get; set; }
        public long? Collected_count { get; set; }

        public TraktMovieResource Movie { get; set; }
    }

    public class RefreshRequestResponse
    {
        public string Access_token { get; set; }
        public string Token_type { get; set; }
        public int Expires_in { get; set; }
        public string Refresh_token { get; set; }
        public string Scope { get; set; }
    }

    public class UserSettingsResponse
    {
        public TraktUserResource User { get; set; }
    }

    public class TraktUserResource
    {
        public string Username { get; set; }
        public TraktUserIdsResource Ids { get; set; }
    }

    public class TraktUserIdsResource
    {
        public string Slug { get; set; }
    }
}
