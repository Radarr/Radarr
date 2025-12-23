using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Configuration
{
    public interface IDownloadConfigService
    {
        int Retention { get; set; }
        string RecycleBin { get; set; }
        int RecycleBinCleanupDays { get; set; }
        ProperDownloadTypes DownloadPropersAndRepacks { get; set; }
        bool EnableCompletedDownloadHandling { get; set; }
        bool AutoRedownloadFailed { get; set; }
        bool AutoRedownloadFailedFromInteractiveSearch { get; set; }
        string DownloadClientWorkingFolders { get; set; }
        int CheckForFinishedDownloadInterval { get; set; }
        int DownloadClientHistoryLimit { get; set; }
        bool PreferIndexerFlags { get; set; }
        bool AllowHardcodedSubs { get; set; }
        string WhitelistedHardcodedSubs { get; set; }
        int MaximumSize { get; set; }
        int MinimumAge { get; set; }
    }
}
