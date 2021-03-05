using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public interface INotification : IProvider
    {
        string Link { get; }

        void OnGrab(GrabMessage grabMessage);
        void OnDownload(DownloadMessage message);
        void OnMovieRename(Movie movie);
        void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage);
        void OnMovieDelete(MovieDeleteMessage deleteMessage);
        void OnHealthIssue(HealthCheck.HealthCheck healthCheck);
        void ProcessQueue();
        bool SupportsOnGrab { get; }
        bool SupportsOnDownload { get; }
        bool SupportsOnUpgrade { get; }
        bool SupportsOnRename { get; }
        bool SupportsOnMovieDelete { get; }
        bool SupportsOnMovieFileDelete { get; }
        bool SupportsOnMovieFileDeleteForUpgrade { get; }
        bool SupportsOnHealthIssue { get; }
    }
}
