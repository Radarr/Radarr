using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.History
{
    public class MovieHistory : ModelBase
    {
        public const string DOWNLOAD_CLIENT = "downloadClient";
        public const string MOVIE_MATCH_TYPE = "movieMatchType";
        public const string RELEASE_SOURCE = "releaseSource";

        public MovieHistory()
        {
            Data = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public int MovieId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public Movie Movie { get; set; }
        public MovieHistoryEventType EventType { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public List<Language> Languages { get; set; }

        public string DownloadId { get; set; }
    }

    public enum MovieHistoryEventType
    {
        Unknown = 0,
        Grabbed = 1,

        // SeriesFolderImported = 2, // deprecated
        DownloadFolderImported = 3,
        DownloadFailed = 4,

        // EpisodeFileDeleted = 5, // deprecated
        MovieFileDeleted = 6,
        MovieFolderImported = 7, // not used yet
        MovieFileRenamed = 8,
        DownloadIgnored = 9
    }
}
