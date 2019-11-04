using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications
{
    public interface INotification : IProvider
    {
        string Link { get; }

        void OnGrab(GrabMessage grabMessage);
        void OnDownload(DownloadMessage message);
        void OnMovieRename(Movie movie);
        void OnHealthIssue(HealthCheck.HealthCheck healthCheck);
        bool SupportsOnGrab { get; }
        bool SupportsOnDownload { get; }
        bool SupportsOnUpgrade { get; }
        bool SupportsOnRename { get; }
        bool SupportsOnHealthIssue { get; }
    }
}
