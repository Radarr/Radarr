﻿using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Common.Http.Proxy;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Configuration
{
    public interface IConfigService
    {
        void SaveConfigDictionary(Dictionary<string, object> configValues);

        bool IsDefined(string key);

        //Download Client
        string DownloadedMoviesFolder { get; set; }
        string DownloadClientWorkingFolders { get; set; }
        int DownloadedMoviesScanInterval { get; set; }
        int DownloadClientHistoryLimit { get; set; }

        //Completed/Failed Download Handling (Download client)
        bool EnableCompletedDownloadHandling { get; set; }
        bool RemoveCompletedDownloads { get; set; }

        bool AutoRedownloadFailed { get; set; }
        bool RemoveFailedDownloads { get; set; }

        //Media Management
        bool AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        string RecycleBin { get; set; }
        bool AutoDownloadPropers { get; set; }
        bool CreateEmptySeriesFolders { get; set; }
        FileDateType FileDate { get; set; }
        bool SkipFreeSpaceCheckWhenImporting { get; set; }
        bool CopyUsingHardlinks { get; set; }
        bool EnableMediaInfo { get; set; }
        string ExtraFileExtensions { get; set; }
        bool AutoRenameFolders { get; set; }
        bool PathsDefaultStatic { get; set; }

        //Permissions (Media Management)
        bool SetPermissionsLinux { get; set; }
        string FileChmod { get; set; }
        string FolderChmod { get; set; }
        string ChownUser { get; set; }
        string ChownGroup { get; set; }

        //Indexers
        int Retention { get; set; }
        int RssSyncInterval { get; set; }
        int MinimumAge { get; set; }

        bool PreferIndexerFlags { get; set; }

		int AvailabilityDelay { get; set; }

		bool AllowHardcodedSubs { get; set; }
		string WhitelistedHardcodedSubs { get; set; }
        ParsingLeniencyType ParsingLeniency { get; set; }

        int NetImportSyncInterval { get; set; }
	string ListSyncLevel { get; set; }
	string ImportExclusions { get; set; }
		string TmdbSessionId { get; set; }
        string TmdbRequestToken { get; set; }
        string TraktAuthToken { get; set; }
        string TraktRefreshToken { get; set; }
        int TraktTokenExpiry { get; set; }
		string NewTraktAuthToken { get; set; }
		string NewTraktRefreshToken {get; set; }
		int NewTraktTokenExpiry { get; set; }

        //UI
        int FirstDayOfWeek { get; set; }
        string CalendarWeekColumnHeader { get; set; }

        string ShortDateFormat { get; set; }
        string LongDateFormat { get; set; }
        string TimeFormat { get; set; }
        bool ShowRelativeDates { get; set; }

        bool EnableColorImpairedMode { get; set; }

        //Internal
        bool CleanupMetadataImages { get; set; }


        //Forms Auth
        string RijndaelPassphrase { get; }
        string HmacPassphrase { get; }
        string RijndaelSalt { get; }
        string HmacSalt { get; }

        //Proxy
        bool ProxyEnabled { get; }
        ProxyType ProxyType { get; }
        string ProxyHostname { get; }
        int ProxyPort { get; }
        string ProxyUsername { get; }
        string ProxyPassword { get; }
        string ProxyBypassFilter { get; }
        bool ProxyBypassLocalAddresses { get; }
    }
}
