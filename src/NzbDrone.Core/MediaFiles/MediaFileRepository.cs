using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.MediaFiles
{
    public interface IMediaFileRepository : IBasicRepository<TrackFile>
    {
        List<TrackFile> GetFilesByArtist(int artistId);
        List<TrackFile> GetFilesByAlbum(int albumId);
        List<TrackFile> GetFilesWithoutMediaInfo();
        List<TrackFile> GetFilesWithRelativePath(int artistId, string relativePath);
    }


    public class MediaFileRepository : BasicRepository<TrackFile>, IMediaFileRepository
    {
        public MediaFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<TrackFile> GetFilesWithoutMediaInfo()
        {
            return Query.Where(c => c.MediaInfo == null).ToList();
        }

        public List<TrackFile> GetFilesByArtist(int artistId)
        {
            string query = string.Format("SELECT TrackFiles.* " +
                                         "FROM Artists " +
                                         "JOIN Albums ON Albums.ArtistMetadataId = Artists.ArtistMetadataId " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "JOIN TrackFiles ON TrackFiles.Id == Tracks.TrackFileId " +
                                         "WHERE Artists.Id == {0} " +
                                         "AND AlbumReleases.Monitored = 1",
                                         artistId);

            return Query.QueryText(query).ToList();
        }

        public List<TrackFile> GetFilesByAlbum(int albumId)
        {
            return Query.Where(c => c.AlbumId == albumId).ToList();
        }
        
        public List<TrackFile> GetFilesWithRelativePath(int artistId, string relativePath)
        {
            var mapper = DataMapper;
            mapper.AddParameter("artistId", artistId);
            mapper.AddParameter("relativePath", relativePath);
            string query = "SELECT TrackFiles.* " +
                "FROM Artists " +
                "JOIN Albums ON Albums.ArtistMetadataId = Artists.ArtistMetadataId " +
                "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                "JOIN TrackFiles ON TrackFiles.Id == Tracks.TrackFileId " +
                "WHERE Artists.Id == @artistId " +
                "AND AlbumReleases.Monitored = 1 " +
                "AND TrackFiles.RelativePath == @relativePath";

            return mapper.Query<TrackFile>(query);
        }

    }
}
