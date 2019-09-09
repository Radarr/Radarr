using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Xbmc.Model
{
    public class KodiArtist
    {
        public int ArtistId { get; set; }
        public string Label { get; set; }
        public List<string> MusicbrainzArtistId { get; set; }
        public string File { get; set; }
    }
}
