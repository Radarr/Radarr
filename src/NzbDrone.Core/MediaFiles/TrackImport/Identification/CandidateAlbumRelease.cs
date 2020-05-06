using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.TrackImport.Identification
{
    public class CandidateAlbumRelease
    {
        public CandidateAlbumRelease()
        {
        }

        public CandidateAlbumRelease(Book book)
        {
            Book = book;
            ExistingTracks = new List<BookFile>();
        }

        public Book Book { get; set; }
        public List<BookFile> ExistingTracks { get; set; }
    }
}
