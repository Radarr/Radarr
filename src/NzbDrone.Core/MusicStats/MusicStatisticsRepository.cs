using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MusicStats
{
    public interface IMusicStatisticsRepository
    {
        List<MusicStatistics> AlbumStatistics();
        List<MusicStatistics> AlbumStatistics(int albumId);
    }

    public class MusicStatisticsRepository : IMusicStatisticsRepository
    {
        private const string _selectAlbumsTemplate = "SELECT /**select**/ FROM \"Albums\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
        private const string _selectMusicFilesTemplate = "SELECT /**select**/ FROM \"MusicFiles\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public MusicStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<MusicStatistics> AlbumStatistics()
        {
            return MapResults(Query(AlbumsBuilder(), _selectAlbumsTemplate),
                Query(MusicFilesBuilder(), _selectMusicFilesTemplate));
        }

        public List<MusicStatistics> AlbumStatistics(int albumId)
        {
            return MapResults(Query(AlbumsBuilder().Where<Album>(x => x.Id == albumId), _selectAlbumsTemplate),
                Query(MusicFilesBuilder().Where<MusicFile>(x => x.AlbumId == albumId), _selectMusicFilesTemplate));
        }

        private static List<MusicStatistics> MapResults(List<MusicStatistics> albumsResult, List<MusicStatistics> filesResult)
        {
            albumsResult.ForEach(e =>
            {
                var file = filesResult.SingleOrDefault(f => f.AlbumId == e.AlbumId);

                e.SizeOnDisk = file?.SizeOnDisk ?? 0;
                e.TrackFileCount = file?.TrackFileCount ?? 0;
                e.ReleaseGroupsString = file?.ReleaseGroupsString;
            });

            return albumsResult;
        }

        private List<MusicStatistics> Query(SqlBuilder builder, string template)
        {
            var sql = builder.AddTemplate(template).LogQuery();

            using var conn = _database.OpenConnection();

            return conn.Query<MusicStatistics>(sql.RawSql, sql.Parameters).ToList();
        }

        private SqlBuilder AlbumsBuilder()
        {
            return new SqlBuilder(_database.DatabaseType)
                .Select(@"""Albums"".""Id"" AS AlbumId,
                        COUNT(""Tracks"".""Id"") AS TrackCount")
                .LeftJoin<Album, Track>((a, t) => a.Id == t.AlbumId)
                .GroupBy<Album>(x => x.Id);
        }

        private SqlBuilder MusicFilesBuilder()
        {
            if (_database.DatabaseType == DatabaseType.SQLite)
            {
                return new SqlBuilder(_database.DatabaseType)
                    .Select(@"""AlbumId"",
                            COUNT(""Id"") AS TrackFileCount,
                            SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                            GROUP_CONCAT(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                    .GroupBy<MusicFile>(x => x.AlbumId);
            }

            return new SqlBuilder(_database.DatabaseType)
                .Select(@"""AlbumId"",
                        COUNT(""Id"") AS TrackFileCount,
                        SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                        string_agg(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                .GroupBy<MusicFile>(x => x.AlbumId);
        }
    }
}
