namespace NzbDrone.Core.Notifications.Trakt.Resource
{
    public class TraktMovieResource
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public TraktMovieIdsResource Ids { get; set; }
    }
}
