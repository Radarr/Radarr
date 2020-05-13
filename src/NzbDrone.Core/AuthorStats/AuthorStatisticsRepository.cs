using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.AuthorStats
{
    public interface IAuthorStatisticsRepository
    {
        List<BookStatistics> AuthorStatistics();
        List<BookStatistics> AuthorStatistics(int authorId);
    }

    public class AuthorStatisticsRepository : IAuthorStatisticsRepository
    {
        private const string _selectTemplate = "SELECT /**select**/ FROM Books /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public AuthorStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<BookStatistics> AuthorStatistics()
        {
            var time = DateTime.UtcNow;
            return Query(Builder().Where<Book>(x => x.ReleaseDate < time));
        }

        public List<BookStatistics> AuthorStatistics(int authorId)
        {
            var time = DateTime.UtcNow;
            return Query(Builder().Where<Book>(x => x.ReleaseDate < time)
                         .Where<Author>(x => x.Id == authorId));
        }

        private List<BookStatistics> Query(SqlBuilder builder)
        {
            var sql = builder.AddTemplate(_selectTemplate).LogQuery();

            using (var conn = _database.OpenConnection())
            {
                return conn.Query<BookStatistics>(sql.RawSql, sql.Parameters).ToList();
            }
        }

        private SqlBuilder Builder() => new SqlBuilder()
            .Select(@"Authors.Id AS AuthorId,
                     Books.Id AS BookId,
                     SUM(COALESCE(BookFiles.Size, 0)) AS SizeOnDisk,
                     COUNT(Books.Id) AS TotalBookCount,
                     SUM(CASE WHEN BookFiles.Id IS NULL THEN 0 ELSE 1 END) AS AvailableBookCount,
                     SUM(CASE WHEN Books.Monitored = 1 OR BookFiles.Id IS NOT NULL THEN 1 ELSE 0 END) AS BookCount,
                     SUM(CASE WHEN BookFiles.Id IS NULL THEN 0 ELSE 1 END) AS BookFileCount")
            .Join<Book, Author>((book, author) => book.AuthorMetadataId == author.AuthorMetadataId)
            .LeftJoin<Book, BookFile>((t, f) => t.Id == f.BookId)
            .GroupBy<Author>(x => x.Id)
            .GroupBy<Book>(x => x.Id);
    }
}
