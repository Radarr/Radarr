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

        public WebhookRemoteMovie()
        {
        }

        public WebhookRemoteMovie(RemoteMovie remoteMovie)
        {
            TmdbId = remoteMovie.Movie.MovieMetadata.Value.TmdbId;
            ImdbId = remoteMovie.Movie.MovieMetadata.Value.ImdbId;
            Title = remoteMovie.Movie.MovieMetadata.Value.Title;
            Year = remoteMovie.Movie.MovieMetadata.Value.Year;
        }

        public WebhookRemoteMovie(Movie movie)
        {
            TmdbId = movie.MovieMetadata.Value.TmdbId;
            ImdbId = movie.MovieMetadata.Value.ImdbId;
            Title = movie.MovieMetadata.Value.Title;
            Year = movie.MovieMetadata.Value.Year;
        }
    }
}
