using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileRepository<TExtraFile> : IBasicRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        void DeleteForAuthor(int authorId);
        void DeleteForBook(int authorId, int bookId);
        void DeleteForBookFile(int bookFileId);
        List<TExtraFile> GetFilesByAuthor(int authorId);
        List<TExtraFile> GetFilesByBook(int authorId, int bookId);
        List<TExtraFile> GetFilesByBookFile(int bookFileId);
        TExtraFile FindByPath(string path);
    }

    public class ExtraFileRepository<TExtraFile> : BasicRepository<TExtraFile>, IExtraFileRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        public ExtraFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteForAuthor(int authorId)
        {
            Delete(c => c.AuthorId == authorId);
        }

        public void DeleteForBook(int authorId, int bookId)
        {
            Delete(c => c.AuthorId == authorId && c.BookId == bookId);
        }

        public void DeleteForBookFile(int bookFileId)
        {
            Delete(c => c.BookFileId == bookFileId);
        }

        public List<TExtraFile> GetFilesByAuthor(int authorId)
        {
            return Query(c => c.AuthorId == authorId);
        }

        public List<TExtraFile> GetFilesByBook(int authorId, int bookId)
        {
            return Query(c => c.AuthorId == authorId && c.BookId == bookId);
        }

        public List<TExtraFile> GetFilesByBookFile(int bookFileId)
        {
            return Query(c => c.BookFileId == bookFileId);
        }

        public TExtraFile FindByPath(string path)
        {
            return Query(c => c.RelativePath == path).SingleOrDefault();
        }
    }
}
