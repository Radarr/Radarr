using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Blacklisting
{
    public class Blacklist : ModelBase
    {
        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public DateTime? PublishedDate { get; set; }
        public long? Size { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }
        public string TorrentInfoHash { get; set; }
    }
}
