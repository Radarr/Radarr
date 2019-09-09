using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications
{
    public interface INotification : IProvider
    {
        string Link { get; }

        void OnGrab(GrabMessage grabMessage);
        void OnReleaseImport(AlbumDownloadMessage message);
        void OnRename(Artist artist);
        void OnHealthIssue(HealthCheck.HealthCheck healthCheck);
        void OnDownloadFailure(DownloadFailedMessage message);
        void OnImportFailure(AlbumDownloadMessage message);
        void OnTrackRetag(TrackRetagMessage message);
        bool SupportsOnGrab { get; }
        bool SupportsOnReleaseImport { get; }
        bool SupportsOnUpgrade { get; }
        bool SupportsOnRename { get; }
        bool SupportsOnHealthIssue { get; }
        bool SupportsOnDownloadFailure { get; }
        bool SupportsOnImportFailure { get; }
        bool SupportsOnTrackRetag { get; }
    }
}
