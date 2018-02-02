using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class GrabMessage
    {
        public string Message { get; set; }
        public Movie Movie { get; set; }
        public RemoteMovie RemoteMovie { get; set; }
        public QualityModel Quality { get; set; }   

        public override string ToString()
        {
            return Message;
        }
    }
}
