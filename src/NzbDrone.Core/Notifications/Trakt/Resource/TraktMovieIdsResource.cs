namespace NzbDrone.Core.Notifications.Trakt.Resource
{
    public class TraktMovieIdsResource
    {
        public int Trakt { get; set; }
        public string Slug { get; set; }
        public string Imdb { get; set; }
        public int Tmdb { get; set; }
    }
}
