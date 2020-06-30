using System.Collections.Generic;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MediaFiles.BookImport.Identification
{
    public class CandidateEdition
    {
        public CandidateEdition()
        {
        }

        public CandidateEdition(Edition edition)
        {
            Edition = edition;
            ExistingFiles = new List<BookFile>();
        }

        public Edition Edition { get; set; }
        public List<BookFile> ExistingFiles { get; set; }
    }
}
