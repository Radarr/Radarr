using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Download
{
    public class DownloadIgnoredEvent : IEvent
    {
        public int AuthorId { get; set; }
        public List<int> BookIds { get; set; }
        public QualityModel Quality { get; set; }
        public string SourceTitle { get; set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; set; }
        public string Message { get; set; }
    }
}
