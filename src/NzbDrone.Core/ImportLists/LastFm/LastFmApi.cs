using System.Collections.Generic;

namespace NzbDrone.Core.ImportLists.LastFm
{
    public class LastFmArtistList
    {
        public List<LastFmArtist> Artist { get; set; }
    }

    public class LastFmArtistResponse
    {
        public LastFmArtistList TopArtists { get; set; }
    }

    public class LastFmArtist
    {
        public string Name { get; set; }
        public string Mbid { get; set; }
    }
}
