using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class BookFileMoveResult
    {
        public BookFileMoveResult()
        {
            OldFiles = new List<BookFile>();
        }

        public BookFile BookFile { get; set; }
        public List<BookFile> OldFiles { get; set; }
    }
}
