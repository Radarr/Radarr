using System.Collections.Generic;
using System.Linq;
using Marr.Data;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<TrackFile>
    {
        List<TrackFile> GetFilesByArtist(int artistId);
        List<TrackFile> GetFilesByAlbum(int albumId);
        List<TrackFile> GetFilesByRelease(int releaseId);
        List<TrackFile> GetUnmappedFiles();
        List<TrackFile> GetFilesWithBasePath(string path);
        TrackFile GetFileWithPath(string path);
        void DeleteFilesByAlbum(int albumId);
    }


    public class MediaFileRepository : BasicRepository<TrackFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        // always join with all the other good stuff
        // needed more often than not so better to load it all now
        protected override QueryBuilder<TrackFile> Query =>
            DataMapper.Query<TrackFile>()
            .Join<TrackFile, Track>(JoinType.Left, t => t.Tracks, (t, x) => t.Id == x.TrackFileId)
            .Join<TrackFile, Album>(JoinType.Left, t => t.Album, (t, a) => t.AlbumId == a.Id)
            .Join<TrackFile, Artist>(JoinType.Left, t => t.Artist, (t, a) => t.Album.Value.ArtistMetadataId == a.ArtistMetadataId)
            .Join<Artist, ArtistMetadata>(JoinType.Left, a => a.Metadata, (a, m) => a.ArtistMetadataId == m.Id);

        public List<TrackFile> GetFilesByArtist(int artistId)
        {
            return Query
                .Join<Track, AlbumRelease>(JoinType.Inner, t => t.AlbumRelease, (t, r) => t.AlbumReleaseId == r.Id)
                .Where<AlbumRelease>(r => r.Monitored == true)
                .AndWhere(t => t.Artist.Value.Id == artistId)
                .ToList();
        }

        public List<TrackFile> GetFilesByAlbum(int albumId)
        {
            return Query
                .Join<Track, AlbumRelease>(JoinType.Inner, t => t.AlbumRelease, (t, r) => t.AlbumReleaseId == r.Id)
                .Where<AlbumRelease>(r => r.Monitored == true)
                .AndWhere(f => f.AlbumId == albumId)
                .ToList();
        }

        public List<TrackFile> GetUnmappedFiles()
        {
            var query = "SELECT TrackFiles.* " +
                        "FROM TrackFiles " +
                        "LEFT JOIN Tracks ON Tracks.TrackFileId = TrackFiles.Id " +
                        "WHERE Tracks.Id IS NULL ";

            return DataMapper.Query<TrackFile>().QueryText(query).ToList();
        }

        public void DeleteFilesByAlbum(int albumId)
        {
            var ids = DataMapper.Query<TrackFile>().Where(x => x.AlbumId == albumId);
            DeleteMany(ids);
        }

        public List<TrackFile> GetFilesByRelease(int releaseId)
        {
            return Query
                .Where<Track>(x => x.AlbumReleaseId == releaseId)
                .ToList();
        }
        
        public List<TrackFile> GetFilesWithBasePath(string path)
        {
            return Query
                .Where(x => x.Path.StartsWith(path))
                .ToList();
        }
        
        public TrackFile GetFileWithPath(string path)
        {
            return Query.Where(x => x.Path == path).SingleOrDefault();
        }
    }
}
