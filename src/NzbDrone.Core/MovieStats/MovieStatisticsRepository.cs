using System;
using System.Collections.Generic;
using System.Text;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MovieStats
{
    public interface IMovieStatisticsRepository
    {
        List<SeasonStatistics> MovieStatistics();
        List<SeasonStatistics> MovieStatistics(int movieId);
    }

    public class MovieStatisticsRepository : IMovieStatisticsRepository
    {
        private readonly IMainDatabase _database;

        public MovieStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<SeasonStatistics> MovieStatistics()
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("currentDate", DateTime.UtcNow);

            var sb = new StringBuilder();
            sb.AppendLine(GetSelectClause());
            sb.AppendLine(GetEpisodeFilesJoin());
            sb.AppendLine(GetGroupByClause());
            var queryText = sb.ToString();

            return new List<SeasonStatistics>();

            return mapper.Query<SeasonStatistics>(queryText);
        }

        public List<SeasonStatistics> MovieStatistics(int movieId)
        {
            var mapper = _database.GetDataMapper();

            mapper.AddParameter("currentDate", DateTime.UtcNow);
            mapper.AddParameter("movieId", movieId);

            var sb = new StringBuilder();
            sb.AppendLine(GetSelectClause());
            sb.AppendLine(GetEpisodeFilesJoin());
            sb.AppendLine("WHERE Episodes.MovieId = @movieId");
            sb.AppendLine(GetGroupByClause());
            var queryText = sb.ToString();

            return new List<SeasonStatistics>();

            return mapper.Query<SeasonStatistics>(queryText);
        }

        private string GetSelectClause()
        {
            return @"SELECT Episodes.*, SUM(EpisodeFiles.Size) as SizeOnDisk FROM
                     (SELECT
                     Episodes.MovieId,
                     Episodes.SeasonNumber,
                     SUM(CASE WHEN AirdateUtc <= @currentDate OR EpisodeFileId > 0 THEN 1 ELSE 0 END) AS TotalEpisodeCount,
                     SUM(CASE WHEN (Monitored = 1 AND AirdateUtc <= @currentDate) OR EpisodeFileId > 0 THEN 1 ELSE 0 END) AS EpisodeCount,
                     SUM(CASE WHEN EpisodeFileId > 0 THEN 1 ELSE 0 END) AS EpisodeFileCount,
                     MIN(CASE WHEN AirDateUtc < @currentDate OR EpisodeFileId > 0 OR Monitored = 0 THEN NULL ELSE AirDateUtc END) AS NextAiringString,
                     MAX(CASE WHEN AirDateUtc >= @currentDate OR EpisodeFileId = 0 AND Monitored = 0 THEN NULL ELSE AirDateUtc END) AS PreviousAiringString
                     FROM Episodes
                     GROUP BY Episodes.MovieId, Episodes.SeasonNumber) as Episodes";
        }

        private string GetGroupByClause()
        {
            return "GROUP BY Episodes.MovieId, Episodes.SeasonNumber";
        }

        private string GetEpisodeFilesJoin()
        {
            return @"LEFT OUTER JOIN EpisodeFiles
                     ON EpisodeFiles.MovieId = Episodes.MovieId
                     AND EpisodeFiles.SeasonNumber = Episodes.SeasonNumber";
        }
    }
}
