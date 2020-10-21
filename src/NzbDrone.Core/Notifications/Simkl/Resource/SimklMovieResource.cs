namespace NzbDrone.Core.Notifications.Simkl.Resource
{
    public class SimklMovieResource
    {
        public string Title { get; set; }
        public int? Year { get; set; }
        public SimklMovieIdsResource Ids { get; set; }
    }
}
