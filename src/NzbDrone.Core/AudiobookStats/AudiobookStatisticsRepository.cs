using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Audiobooks;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.AudiobookStats
{
    public interface IAudiobookStatisticsRepository
    {
        List<AudiobookStatistics> AudiobookStatistics();
        List<AudiobookStatistics> AudiobookStatistics(int audiobookId);
    }

    public class AudiobookStatisticsRepository : IAudiobookStatisticsRepository
    {
        private const string _selectAudiobooksTemplate = "SELECT /**select**/ FROM \"Audiobooks\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
        private const string _selectAudiobookFilesTemplate = "SELECT /**select**/ FROM \"AudiobookFiles\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public AudiobookStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<AudiobookStatistics> AudiobookStatistics()
        {
            return MapResults(Query(AudiobooksBuilder(), _selectAudiobooksTemplate),
                Query(AudiobookFilesBuilder(), _selectAudiobookFilesTemplate));
        }

        public List<AudiobookStatistics> AudiobookStatistics(int audiobookId)
        {
            return MapResults(Query(AudiobooksBuilder().Where<Audiobook>(x => x.Id == audiobookId), _selectAudiobooksTemplate),
                Query(AudiobookFilesBuilder().Where<AudiobookFile>(x => x.AudiobookId == audiobookId), _selectAudiobookFilesTemplate));
        }

        private static List<AudiobookStatistics> MapResults(List<AudiobookStatistics> audiobooksResult, List<AudiobookStatistics> filesResult)
        {
            audiobooksResult.ForEach(e =>
            {
                var file = filesResult.SingleOrDefault(f => f.AudiobookId == e.AudiobookId);

                e.SizeOnDisk = file?.SizeOnDisk ?? 0;
                e.TotalDurationSeconds = file?.TotalDurationSeconds ?? 0;
                e.ReleaseGroupsString = file?.ReleaseGroupsString;
            });

            return audiobooksResult;
        }

        private List<AudiobookStatistics> Query(SqlBuilder builder, string template)
        {
            var sql = builder.AddTemplate(template).LogQuery();

            using var conn = _database.OpenConnection();

            return conn.Query<AudiobookStatistics>(sql.RawSql, sql.Parameters).ToList();
        }

        private SqlBuilder AudiobooksBuilder()
        {
            return new SqlBuilder(_database.DatabaseType)
                .Select(@"""Audiobooks"".""Id"" AS AudiobookId,
                        COUNT(""AudiobookFiles"".""Id"") AS AudiobookFileCount")
                .LeftJoin<Audiobook, AudiobookFile>((a, af) => a.Id == af.AudiobookId)
                .GroupBy<Audiobook>(x => x.Id);
        }

        private SqlBuilder AudiobookFilesBuilder()
        {
            if (_database.DatabaseType == DatabaseType.SQLite)
            {
                return new SqlBuilder(_database.DatabaseType)
                    .Select(@"""AudiobookId"",
                            SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                            SUM(COALESCE(""DurationSeconds"", 0)) AS TotalDurationSeconds,
                            GROUP_CONCAT(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                    .GroupBy<AudiobookFile>(x => x.AudiobookId);
            }

            return new SqlBuilder(_database.DatabaseType)
                .Select(@"""AudiobookId"",
                        SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                        SUM(COALESCE(""DurationSeconds"", 0)) AS TotalDurationSeconds,
                        string_agg(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                .GroupBy<AudiobookFile>(x => x.AudiobookId);
        }
    }
}
