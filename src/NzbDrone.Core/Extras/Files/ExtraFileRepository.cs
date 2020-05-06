using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileRepository<TExtraFile> : IBasicRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        void DeleteForArtist(int authorId);
        void DeleteForAlbum(int authorId, int bookId);
        void DeleteForTrackFile(int trackFileId);
        List<TExtraFile> GetFilesByArtist(int authorId);
        List<TExtraFile> GetFilesByAlbum(int authorId, int bookId);
        List<TExtraFile> GetFilesByTrackFile(int trackFileId);
        TExtraFile FindByPath(string path);
    }

    public class ExtraFileRepository<TExtraFile> : BasicRepository<TExtraFile>, IExtraFileRepository<TExtraFile>
        where TExtraFile : ExtraFile, new()
    {
        public ExtraFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public void DeleteForArtist(int authorId)
        {
            Delete(c => c.AuthorId == authorId);
        }

        public void DeleteForAlbum(int authorId, int bookId)
        {
            Delete(c => c.AuthorId == authorId && c.BookId == bookId);
        }

        public void DeleteForTrackFile(int trackFileId)
        {
            Delete(c => c.TrackFileId == trackFileId);
        }

        public List<TExtraFile> GetFilesByArtist(int authorId)
        {
            return Query(c => c.AuthorId == authorId);
        }

        public List<TExtraFile> GetFilesByAlbum(int authorId, int bookId)
        {
            return Query(c => c.AuthorId == authorId && c.BookId == bookId);
        }

        public List<TExtraFile> GetFilesByTrackFile(int trackFileId)
        {
            return Query(c => c.TrackFileId == trackFileId);
        }

        public TExtraFile FindByPath(string path)
        {
            return Query(c => c.RelativePath == path).SingleOrDefault();
        }
    }
}
