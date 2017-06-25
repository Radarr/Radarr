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
            sb.AppendLine(GetTrackFilesJoin());
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
            sb.AppendLine(GetTrackFilesJoin());
            sb.AppendLine("WHERE Tracks.ArtistId = @artistId");
            sb.AppendLine(GetGroupByClause());
            var queryText = sb.ToString();

            return mapper.Query<AlbumStatistics>(queryText);
        }

        private string GetSelectClause()
        {
            return @"SELECT Tracks.*, SUM(TrackFiles.Size) as SizeOnDisk FROM
                     (SELECT
                     Tracks.ArtistId,
                     Tracks.AlbumId,
                     SUM(CASE WHEN TrackFileId > 0 THEN 1 ELSE 0 END) AS TotalTrackCount,
                     SUM(CASE WHEN Monitored = 1 OR TrackFileId > 0 THEN 1 ELSE 0 END) AS TrackCount,
                     SUM(CASE WHEN TrackFileId > 0 THEN 1 ELSE 0 END) AS TrackFileCount
                     FROM Tracks
                     GROUP BY Tracks.ArtistId, Tracks.AlbumId) as Tracks";
        }

        private string GetGroupByClause()
        {
            return "GROUP BY Tracks.ArtistId, Tracks.AlbumId";
        }

        private string GetTrackFilesJoin()
        {
            return @"LEFT OUTER JOIN TrackFiles
                     ON TrackFiles.ArtistId = Tracks.ArtistId
                     AND TrackFiles.AlbumId = Tracks.AlbumId";
        }
    }
}
