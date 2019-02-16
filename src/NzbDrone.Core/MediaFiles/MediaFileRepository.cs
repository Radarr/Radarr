using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<TrackFile>
    {
        List<TrackFile> GetFilesByArtist(int artistId);
        List<TrackFile> GetFilesByAlbum(int albumId);
        List<TrackFile> GetFilesWithRelativePath(int artistId, string relativePath);
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
            .Join<TrackFile, Track>(JoinType.Inner, t => t.Tracks, (t, x) => t.Id == x.TrackFileId)
            .Join<TrackFile, Album>(JoinType.Inner, t => t.Album, (t, a) => t.AlbumId == a.Id)
            .Join<TrackFile, Artist>(JoinType.Inner, t => t.Artist, (t, a) => t.Album.Value.ArtistMetadataId == a.ArtistMetadataId)
            .Join<Artist, ArtistMetadata>(JoinType.Inner, a => a.Metadata, (a, m) => a.ArtistMetadataId == m.Id);

        public List<TrackFile> GetFilesByArtist(int artistId)
        {
            return Query
                .Join<Album, AlbumRelease>(JoinType.Inner, a => a.AlbumReleases, (a, r) => a.Id == r.AlbumId)
                .Where<AlbumRelease>(r => r.Monitored == true)
                .AndWhere(t => t.Artist.Value.Id == artistId)
                .ToList();
        }

        public List<TrackFile> GetFilesByAlbum(int albumId)
        {
            return Query
                .Where(f => f.AlbumId == albumId)
                .ToList();
        }
        
        public List<TrackFile> GetFilesWithRelativePath(int artistId, string relativePath)
        {
            return Query
                .Join<Album, AlbumRelease>(JoinType.Inner, a => a.AlbumReleases, (a, r) => a.Id == r.AlbumId)
                .Where<AlbumRelease>(r => r.Monitored == true)
                .AndWhere(t => t.Artist.Value.Id == artistId)
                .AndWhere(t => t.RelativePath == relativePath)
                .ToList();
        }
    }
}
