using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Configuration
{
    public class DownloadConfigService : IDownloadConfigService
    {
        private readonly IConfigService _configService;

        public DownloadConfigService(IConfigService configService)
        {
            _configService = configService;
        }

        public int Retention
        {
            get => _configService.Retention;
            set => _configService.Retention = value;
        }

        public string RecycleBin
        {
            get => _configService.RecycleBin;
            set => _configService.RecycleBin = value;
        }

        public int RecycleBinCleanupDays
        {
            get => _configService.RecycleBinCleanupDays;
            set => _configService.RecycleBinCleanupDays = value;
        }

        public ProperDownloadTypes DownloadPropersAndRepacks
        {
            get => _configService.DownloadPropersAndRepacks;
            set => _configService.DownloadPropersAndRepacks = value;
        }

        public bool EnableCompletedDownloadHandling
        {
            get => _configService.EnableCompletedDownloadHandling;
            set => _configService.EnableCompletedDownloadHandling = value;
        }

        public bool AutoRedownloadFailed
        {
            get => _configService.AutoRedownloadFailed;
            set => _configService.AutoRedownloadFailed = value;
        }

        public bool AutoRedownloadFailedFromInteractiveSearch
        {
            get => _configService.AutoRedownloadFailedFromInteractiveSearch;
            set => _configService.AutoRedownloadFailedFromInteractiveSearch = value;
        }

        public string DownloadClientWorkingFolders
        {
            get => _configService.DownloadClientWorkingFolders;
            set => _configService.DownloadClientWorkingFolders = value;
        }

        public int CheckForFinishedDownloadInterval
        {
            get => _configService.CheckForFinishedDownloadInterval;
            set => _configService.CheckForFinishedDownloadInterval = value;
        }

        public int DownloadClientHistoryLimit
        {
            get => _configService.DownloadClientHistoryLimit;
            set => _configService.DownloadClientHistoryLimit = value;
        }

        public bool PreferIndexerFlags
        {
            get => _configService.PreferIndexerFlags;
            set => _configService.PreferIndexerFlags = value;
        }

        public bool AllowHardcodedSubs
        {
            get => _configService.AllowHardcodedSubs;
            set => _configService.AllowHardcodedSubs = value;
        }

        public string WhitelistedHardcodedSubs
        {
            get => _configService.WhitelistedHardcodedSubs;
            set => _configService.WhitelistedHardcodedSubs = value;
        }

        public int MaximumSize
        {
            get => _configService.MaximumSize;
            set => _configService.MaximumSize = value;
        }

        public int MinimumAge
        {
            get => _configService.MinimumAge;
            set => _configService.MinimumAge = value;
        }
    }
}
