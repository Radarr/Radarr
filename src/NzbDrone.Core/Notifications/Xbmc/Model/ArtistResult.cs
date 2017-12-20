using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class ArtistResult
    {
        public Dictionary<string, int> Limits { get; set; }
        public List<KodiArtist> Artists;

        public ArtistResult()
        {
            Artists = new List<KodiArtist>();
        }
    }
}
