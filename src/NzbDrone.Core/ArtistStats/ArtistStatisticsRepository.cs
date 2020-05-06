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
        List<AlbumStatistics> ArtistStatistics(int authorId);
    }

    public class ArtistStatisticsRepository : IArtistStatisticsRepository
    {
        private const string _selectTemplate = "SELECT /**select**/ FROM Books /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public ArtistStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<AlbumStatistics> ArtistStatistics()
        {
            var time = DateTime.UtcNow;
            return Query(Builder().Where<Book>(x => x.ReleaseDate < time));
        }

        public List<AlbumStatistics> ArtistStatistics(int authorId)
        {
            var time = DateTime.UtcNow;
            return Query(Builder().Where<Book>(x => x.ReleaseDate < time)
                         .Where<Author>(x => x.Id == authorId));
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
            .Select(@"Authors.Id AS AuthorId,
                     Books.Id AS BookId,
                     SUM(COALESCE(BookFiles.Size, 0)) AS SizeOnDisk,
                     COUNT(Books.Id) AS TotalTrackCount,
                     SUM(CASE WHEN BookFiles.Id IS NULL THEN 0 ELSE 1 END) AS AvailableTrackCount,
                     SUM(CASE WHEN Books.Monitored = 1 OR BookFiles.Id IS NOT NULL THEN 1 ELSE 0 END) AS TrackCount,
                     SUM(CASE WHEN BookFiles.Id IS NULL THEN 0 ELSE 1 END) AS TrackFileCount")
            .Join<Book, Author>((album, artist) => album.AuthorMetadataId == artist.AuthorMetadataId)
            .LeftJoin<Book, BookFile>((t, f) => t.Id == f.BookId)
            .GroupBy<Author>(x => x.Id)
            .GroupBy<Book>(x => x.Id);
    }
}
