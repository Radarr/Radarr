using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Files
{
    public interface IExtraFileRepository<TExtraFile> : IBasicRepository<TExtraFile> where TExtraFile : ExtraFile, new()
    {
        void DeleteForArtist(int artistId);
        void DeleteForAlbum(int artistId, int albumId);
        void DeleteForTrackFile(int trackFileId);
        List<TExtraFile> GetFilesByArtist(int artistId);
        List<TExtraFile> GetFilesByAlbum(int artistId, int albumId);
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

        public void DeleteForArtist(int artistId)
        {
            Delete(c => c.ArtistId == artistId);
        }

        public void DeleteForAlbum(int artistId, int albumId)
        {
            Delete(c => c.ArtistId == artistId && c.AlbumId == albumId);
        }

        public void DeleteForTrackFile(int trackFileId)
        {
            Delete(c => c.TrackFileId == trackFileId);
        }

        public List<TExtraFile> GetFilesByArtist(int artistId)
        {
            return Query.Where(c => c.ArtistId == artistId);
        }

        public List<TExtraFile> GetFilesByAlbum(int artistId, int albumId)
        {
            return Query.Where(c => c.ArtistId == artistId && c.AlbumId == albumId);
        }

        public List<TExtraFile> GetFilesByTrackFile(int trackFileId)
        {
            return Query.Where(c => c.TrackFileId == trackFileId);
        }

        public TExtraFile FindByPath(string path)
        {
            return Query.Where(c => c.RelativePath == path).SingleOrDefault();
        }
    }
}
