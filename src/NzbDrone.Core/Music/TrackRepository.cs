using NzbDrone.Core.Datastore;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore.Extensions;
using System;

namespace NzbDrone.Core.Music
{
    public interface ITrackRepository : IBasicRepository<Track>
    {
        Track Find(int artistId, int albumId, int mediumNumber, int trackNumber);
        List<Track> GetTracks(int artistId);
        List<Track> GetTracksByAlbum(int albumId);
        List<Track> GetTracksByRelease(int albumReleaseId);
        List<Track> GetTracksByForeignReleaseId(string foreignReleaseId);
        List<Track> GetTracksByForeignTrackIds(List<string> foreignTrackId);
        List<Track> GetTracksByMedium(int albumId, int mediumNumber);
        List<Track> GetTracksByFileId(int fileId);
        List<Track> TracksWithFiles(int artistId);
        void SetFileId(int trackId, int fileId);
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

        public Track Find(int artistId, int albumId, int mediumNumber, int trackNumber)
        {
            string query = string.Format("SELECT Tracks.* " +
                                         "FROM Artists " +
                                         "JOIN Albums ON Albums.ArtistMetadataId == Artists.ArtistMetadataId " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "WHERE Artists.Id = {0} " +
                                         "AND Albums.Id = {1} " +
                                         "AND AlbumReleases.Monitored = 1 " +
                                         "AND Tracks.MediumNumber = {2} " +
                                         "AND Tracks.AbsoluteTrackNumber = {3}",
                                         artistId, albumId, mediumNumber, trackNumber);

            return Query.QueryText(query).SingleOrDefault();
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

        public List<Track> GetTracksByForeignReleaseId(string foreignReleaseId)
        {
            string query = string.Format("SELECT Tracks.* " +
                                         "FROM AlbumReleases " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "WHERE AlbumReleases.ForeignReleaseId = '{0}'",
                                         foreignReleaseId);

            return Query.QueryText(query).ToList();
        }

        public List<Track> GetTracksByForeignTrackIds(List<string> ids)
        {
            string query = string.Format("SELECT Tracks.* " +
                                         "FROM Tracks " +
                                         "WHERE ForeignTrackId IN ('{0}')",
                                         string.Join("', '", ids));

            return Query.QueryText(query).ToList();
        }

        public List<Track> GetTracksByMedium(int albumId, int mediumNumber)
        {
            if (mediumNumber < 1)
            {
                return GetTracksByAlbum(albumId);
            }

            string query = string.Format("SELECT Tracks.* " +
                                         "FROM Albums " +
                                         "JOIN AlbumReleases ON AlbumReleases.AlbumId == Albums.Id " +
                                         "JOIN Tracks ON Tracks.AlbumReleaseId == AlbumReleases.Id " +
                                         "WHERE Albums.Id = {0} " +
                                         "AND AlbumReleases.Monitored = 1 " +
                                         "AND Tracks.MediumNumber = {1}",
                                         albumId,
                                         mediumNumber);

            return Query.QueryText(query).ToList();
        }

        public List<Track> GetTracksByFileId(int fileId)
        {
            return Query.Where(e => e.TrackFileId == fileId).ToList();
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

        public void SetFileId(int trackId, int fileId)
        {
            SetFields(new Track { Id = trackId, TrackFileId = fileId }, track => track.TrackFileId);
        }
    }
}
