using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class MovieResult
    {
        public Dictionary<string, int> Limits { get; set; }
        public List<XbmcMovie> Movies;

        public MovieResult()
        {
            Movies = new List<XbmcMovie>();
        }
    }
}
