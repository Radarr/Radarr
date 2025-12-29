using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IBookFileRepository : IBasicRepository<BookFile>
    {
        List<BookFile> GetFilesByBook(int bookId);
        List<BookFile> GetFilesByBooks(IEnumerable<int> bookIds);
        void DeleteForBooks(List<int> bookIds);
        List<BookFile> GetFilesWithRelativePath(int bookId, string relativePath);
    }

    public class BookFileRepository : BasicRepository<BookFile>, IBookFileRepository
    {
        public BookFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<BookFile> GetFilesByBook(int bookId)
        {
            return Query(x => x.BookId == bookId);
        }

        public List<BookFile> GetFilesByBooks(IEnumerable<int> bookIds)
        {
            return Query(x => bookIds.Contains(x.BookId));
        }

        public void DeleteForBooks(List<int> bookIds)
        {
            Delete(x => bookIds.Contains(x.BookId));
        }

        public List<BookFile> GetFilesWithRelativePath(int bookId, string relativePath)
        {
            return Query(c => c.BookId == bookId && c.RelativePath == relativePath);
        }
    }
}
