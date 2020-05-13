using System.Collections.Generic;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MediaFiles.BookImport.Identification
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
