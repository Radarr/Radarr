using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download.Pending
{
    public class PendingRelease : ModelBase
    {
        public int AuthorId { get; set; }
        public string Title { get; set; }
        public DateTime Added { get; set; }
        public ParsedBookInfo ParsedBookInfo { get; set; }
        public ReleaseInfo Release { get; set; }
        public PendingReleaseReason Reason { get; set; }

        //Not persisted
        public RemoteBook RemoteBook { get; set; }
    }
}
