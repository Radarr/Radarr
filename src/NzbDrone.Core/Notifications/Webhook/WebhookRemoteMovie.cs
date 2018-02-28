using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRemoteMovie
    {
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }

        public WebhookRemoteMovie() { }

        public WebhookRemoteMovie(RemoteMovie remoteMovie)
        {
            TmdbId = remoteMovie.Movie.TmdbId;
            ImdbId = remoteMovie.Movie.ImdbId;
            Title = remoteMovie.Movie.Title;
            Year = remoteMovie.Movie.Year;
        }

        public WebhookRemoteMovie(Movie movie)
        {
            TmdbId = movie.TmdbId;
            ImdbId = movie.ImdbId;
            Title = movie.Title;
            Year = movie.Year;
        }
    }
}
