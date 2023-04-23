using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Pending
{
    public class PendingRelease : ModelBase
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public DateTime Added { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public ReleaseInfo Release { get; set; }
        public PendingReleaseReason Reason { get; set; }
        public PendingReleaseAdditionalInfo AdditionalInfo { get; set; }

        // Not persisted
        public RemoteMovie RemoteMovie { get; set; }
    }

    public class PendingReleaseAdditionalInfo
    {
        public MovieMatchType MovieMatchType { get; set; }
        public ReleaseSourceType ReleaseSource { get; set; }
    }
}
