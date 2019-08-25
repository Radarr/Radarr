using NzbDrone.Core.Datastore;
using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface ITrackRepository : IBasicRepository<Track>
    {
        List<Track> GetTracks(int artistId);
        List<Track> GetTracksByAlbum(int albumId);
        List<Track> GetTracksByRelease(int albumReleaseId);
        List<Track> GetTracksByReleases(List<int> albumReleaseId);
        List<Track> GetTracksForRefresh(int albumReleaseId, IEnumerable<string> foreignTrackIds);
        List<Track> GetTracksByFileId(int fileId);
        List<Track> GetTracksByFileId(IEnumerable<int> ids);
        List<Track> TracksWithFiles(int artistId);
        List<Track> TracksWithoutFiles(int albumId);
        void SetFileId(List<Track> tracks);
        void DetachTrackFile(int trackFileId);
    }

    public class TrackRepository : BasicRepository<Track>, ITrackRepository
    {
        private readonly IMainDatabase _database;
        private readonly Logger _logger;

        public TrackRepository(IMainDatabase database, IEventAggregator eventAggregator, Logger logger)
            : base(database, eventAggregator)
        {
            _database = database;
            _logger = logger;
        }

        public List<Track> GetTracks(int artistId)
        {
            string query = string.Format("SELECT Tracks.* " +
                                         "FROM Artists " +
                                         "JOIN Albums ON Albums.ArtistMetadataId == Artists.ArtistMetadataId " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "WHERE Artists.Id = {0} " +
                                         "AND AlbumReleases.Monitored = 1",
                                         artistId);
            
            return Query.QueryText(query).ToList();
        }

        public List<Track> GetTracksByAlbum(int albumId)
        {
            string query = string.Format("SELECT Tracks.* " +
                                         "FROM Albums " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "WHERE Albums.Id = {0} " +
                                         "AND AlbumReleases.Monitored = 1",
                                         albumId);

            return Query.QueryText(query).ToList();
        }

        public List<Track> GetTracksByRelease(int albumReleaseId)
        {
            return Query.Where(t => t.AlbumReleaseId == albumReleaseId).ToList();
        }

        public List<Track> GetTracksByReleases(List<int> albumReleaseIds)
        {
            // this will populate the artist metadata also
            return Query
                .Join<Track, ArtistMetadata>(Marr.Data.QGen.JoinType.Inner, t => t.ArtistMetadata, (l, r) => l.ArtistMetadataId == r.Id)
                .Where($"[AlbumReleaseId] IN ({string.Join(", ", albumReleaseIds)})")
                .ToList();
        }

        public List<Track> GetTracksForRefresh(int albumReleaseId, IEnumerable<string> foreignTrackIds)
        {
            return Query
                .Where(t => t.AlbumReleaseId == albumReleaseId)
                .OrWhere($"[ForeignTrackId] IN ('{string.Join("', '", foreignTrackIds)}')")
                .ToList();
        }

        public List<Track> GetTracksByFileId(int fileId)
        {
            return Query.Where(e => e.TrackFileId == fileId).ToList();
        }

        public List<Track> GetTracksByFileId(IEnumerable<int> ids)
        {
            return Query.Where($"[TrackFileId] IN ({string.Join(", ", ids)})").ToList();
        }

        public List<Track> TracksWithFiles(int artistId)
        {
            string query = string.Format("SELECT Tracks.* " +
                                         "FROM Artists " +
                                         "JOIN Albums ON Albums.ArtistMetadataId = Artists.ArtistMetadataId " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "JOIN TrackFiles ON TrackFiles.Id == Tracks.TrackFileId " +
                                         "WHERE Artists.Id == {0} " +
                                         "AND AlbumReleases.Monitored = 1 ",
                                         artistId);

            return Query.QueryText(query).ToList();
        }

        public List<Track> TracksWithoutFiles(int albumId)
        {
            string query = string.Format("SELECT Tracks.* " +
                                         "FROM Albums " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "LEFT OUTER JOIN TrackFiles ON TrackFiles.Id == Tracks.TrackFileId " +
                                         "WHERE Albums.Id == {0} " +
                                         "AND AlbumReleases.Monitored = 1 " +
                                         "AND TrackFiles.Id IS NULL",
                                         albumId);

            return Query.QueryText(query).ToList();
        }

        public void SetFileId(List<Track> tracks)
        {
            SetFields(tracks, t => t.TrackFileId);
        }

        public void DetachTrackFile(int trackFileId)
        {
            DataMapper.Update<Track>()
                .Where(x => x.TrackFileId == trackFileId)
                .ColumnsIncluding(x => x.TrackFileId)
                .Entity(new Track { TrackFileId = 0 })
                .Execute();
        }
    }
}
