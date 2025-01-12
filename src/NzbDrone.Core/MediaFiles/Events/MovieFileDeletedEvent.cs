using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileDeletedEvent : IEvent
    {
        public MovieFile MovieFile { get; private set; }
        public DeleteMediaFileReason Reason { get; private set; }
        public UpgradeManagementConfigSnapshot UpgradeManagementConfig { get; private set; }

        public MovieFileDeletedEvent(MovieFile movieFile, DeleteMediaFileReason reason, UpgradeManagementConfigSnapshot upgradeManagementConfig)
        {
            MovieFile = movieFile;
            Reason = reason;
            UpgradeManagementConfig = upgradeManagementConfig;
        }
    }

    public class UpgradeManagementConfigSnapshot
    {
        public bool KeepSubtitles { get; init; }
        public bool KeepMetadata { get; init; }
        public bool KeepOthers { get; init; }
    }
}
