using System;

namespace NzbDrone.Core.ImportLists.ReadarrLists
{
    public class ReadarrListsAlbum
    {
        public string ArtistName { get; set; }
        public string AlbumTitle { get; set; }
        public string ArtistId { get; set; }
        public string AlbumId { get; set; }
        public DateTime? ReleaseDate { get; set; }
    }
}
