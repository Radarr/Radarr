using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public class MovieGrabbedEvent : IEvent
    {
        public RemoteMovie Movie { get; private set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }

        public MovieGrabbedEvent(RemoteMovie movie)
        {
            Movie = movie;
        }
    }
}
