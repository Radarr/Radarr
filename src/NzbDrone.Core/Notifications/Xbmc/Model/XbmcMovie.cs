using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class XbmcMovie
    {
        public int movieId { get; set; }
        public string Label { get; set; }
        public string ImdbNumber { get; set; }
        public Dictionary<string, string> UniqueId { get; set; }
        public string File { get; set; }
    }
}
