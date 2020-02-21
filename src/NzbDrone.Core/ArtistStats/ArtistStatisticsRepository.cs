using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.ArtistStats
{
    public interface IArtistStatisticsRepository
    {
        List<AlbumStatistics> ArtistStatistics();
        List<AlbumStatistics> ArtistStatistics(int artistId);
    }

    public class ArtistStatisticsRepository : IArtistStatisticsRepository
    {
        private const string _selectTemplate = "SELECT /**select**/ FROM Tracks /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public ArtistStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<AlbumStatistics> ArtistStatistics()
        {
            var time = DateTime.UtcNow;
            return Query(Builder().Where<Album>(x => x.ReleaseDate < time));
        }

        public List<AlbumStatistics> ArtistStatistics(int artistId)
        {
            var time = DateTime.UtcNow;
            return Query(Builder().Where<Album>(x => x.ReleaseDate < time)
                         .Where<Artist>(x => x.Id == artistId));
        }

        private List<AlbumStatistics> Query(SqlBuilder builder)
        {
            var sql = builder.AddTemplate(_selectTemplate).LogQuery();

            using (var conn = _database.OpenConnection())
            {
                return conn.Query<AlbumStatistics>(sql.RawSql, sql.Parameters).ToList();
            }
        }

        private SqlBuilder Builder() => new SqlBuilder()
            .Select(@"Artists.Id AS ArtistId,
                     Albums.Id AS AlbumId,
                     SUM(COALESCE(TrackFiles.Size, 0)) AS SizeOnDisk,
                     COUNT(Tracks.Id) AS TotalTrackCount,
                     SUM(CASE WHEN Tracks.TrackFileId > 0 THEN 1 ELSE 0 END) AS AvailableTrackCount,
                     SUM(CASE WHEN Albums.Monitored = 1 OR Tracks.TrackFileId > 0 THEN 1 ELSE 0 END) AS TrackCount,
                     SUM(CASE WHEN TrackFiles.Id IS NULL THEN 0 ELSE 1 END) AS TrackFileCount")
            .Join<Track, AlbumRelease>((t, r) => t.AlbumReleaseId == r.Id)
            .Join<AlbumRelease, Album>((r, a) => r.AlbumId == a.Id)
            .Join<Album, Artist>((album, artist) => album.ArtistMetadataId == artist.ArtistMetadataId)
            .LeftJoin<Track, TrackFile>((t, f) => t.TrackFileId == f.Id)
            .Where<AlbumRelease>(x => x.Monitored == true)
            .GroupBy<Artist>(x => x.Id)
            .GroupBy<Album>(x => x.Id);
    }
}
