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
        private const string _selectTemplate = "SELECT /**select**/ FROM Editions /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public AuthorStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<BookStatistics> AuthorStatistics()
        {
            var time = DateTime.UtcNow;
            var stats = Query(Builder());

#pragma warning disable CS0472
            return Query(Builder().OrWhere<Book>(x => x.ReleaseDate < time)
                         .OrWhere<BookFile>(x => x.Id != null));
#pragma warning restore
        }

        public List<BookStatistics> AuthorStatistics(int authorId)
        {
            var time = DateTime.UtcNow;
#pragma warning disable CS0472
            return Query(Builder().OrWhere<Book>(x => x.ReleaseDate < time)
                         .OrWhere<BookFile>(x => x.Id != null)
                         .Where<Author>(x => x.Id == authorId));
#pragma warning restore
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
            .Join<Edition, Book>((e, b) => e.BookId == b.Id)
            .Join<Book, Author>((book, author) => book.AuthorMetadataId == author.AuthorMetadataId)
            .LeftJoin<Edition, BookFile>((t, f) => t.Id == f.EditionId)
            .Where<Edition>(x => x.Monitored == true)
            .GroupBy<Author>(x => x.Id)
            .GroupBy<Book>(x => x.Id);
    }
}
