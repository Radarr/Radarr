using System;
using System.Collections.Generic;
using System.Text;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.ArtistStats
{
    public interface IArtistStatisticsRepository
    {
        List<AlbumStatistics> ArtistStatistics();
        List<AlbumStatistics> ArtistStatistics(int artistId);
    }

    public class ArtistStatisticsRepository : IArtistStatisticsRepository
    {
        private readonly IMainDatabase _database;

        public ArtistStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<AlbumStatistics> ArtistStatistics()
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("currentDate", DateTime.UtcNow);

            var sb = new StringBuilder();
            sb.AppendLine(GetSelectClause());
            sb.AppendLine("AND Albums.ReleaseDate < @currentDate");
            sb.AppendLine(GetGroupByClause());
            var queryText = sb.ToString();

            return mapper.Query<AlbumStatistics>(queryText);
        }

        public List<AlbumStatistics> ArtistStatistics(int artistId)
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("currentDate", DateTime.UtcNow);
            mapper.AddParameter("artistId", artistId);

            var sb = new StringBuilder();
            sb.AppendLine(GetSelectClause());
            sb.AppendLine("AND Artists.Id = @artistId");
            sb.AppendLine("AND Albums.ReleaseDate < @currentDate");
            sb.AppendLine(GetGroupByClause());
            var queryText = sb.ToString();

            return mapper.Query<AlbumStatistics>(queryText);
        }

        private string GetSelectClause()
        {
            return @"SELECT
                     Artists.Id AS ArtistId,
                     Albums.Id AS AlbumId,
                     SUM(COALESCE(TrackFiles.Size, 0)) AS SizeOnDisk,
                     COUNT(Tracks.Id) AS TotalTrackCount,
                     SUM(CASE WHEN Tracks.TrackFileId > 0 THEN 1 ELSE 0 END) AS AvailableTrackCount,
                     SUM(CASE WHEN Albums.Monitored = 1 OR Tracks.TrackFileId > 0 THEN 1 ELSE 0 END) AS TrackCount,
                     SUM(CASE WHEN TrackFiles.Id IS NULL THEN 0 ELSE 1 END) AS TrackFileCount
                     FROM Tracks
                     JOIN AlbumReleases ON Tracks.AlbumReleaseId = AlbumReleases.Id
                     JOIN Albums ON AlbumReleases.AlbumId = Albums.Id
                     JOIN Artists on Albums.ArtistMetadataId = Artists.ArtistMetadataId
                     LEFT OUTER JOIN TrackFiles ON Tracks.TrackFileId = TrackFiles.Id
                     WHERE AlbumReleases.Monitored = 1";
        }

        private string GetGroupByClause()
        {
            return "GROUP BY Artists.Id, Albums.Id";
        }
    }
}
