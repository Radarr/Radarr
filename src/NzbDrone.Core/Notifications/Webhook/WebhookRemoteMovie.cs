using NzbDrone.Core.Tv;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRemoteMovie
    {
        public string ImdbId { get; set; }
        public string Title { get; set; }

        public WebhookRemoteMovie() { }

        public WebhookRemoteMovie(RemoteMovie remoteMovie)
        {
            ImdbId = remoteMovie.Movie.ImdbId;
            Title = remoteMovie.Release.Title;
        }

        public WebhookRemoteMovie(Movie movie)
        {
            ImdbId = movie.ImdbId;
            Title = movie.Title;
        }
    }
}
