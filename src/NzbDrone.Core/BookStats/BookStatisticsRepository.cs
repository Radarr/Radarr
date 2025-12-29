using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.BookStats
{
    public interface IBookStatisticsRepository
    {
        List<BookStatistics> BookStatistics();
        List<BookStatistics> BookStatistics(int bookId);
    }

    public class BookStatisticsRepository : IBookStatisticsRepository
    {
        private const string _selectBooksTemplate = "SELECT /**select**/ FROM \"Books\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";
        private const string _selectBookFilesTemplate = "SELECT /**select**/ FROM \"BookFiles\" /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/ /**orderby**/";

        private readonly IMainDatabase _database;

        public BookStatisticsRepository(IMainDatabase database)
        {
            _database = database;
        }

        public List<BookStatistics> BookStatistics()
        {
            return MapResults(Query(BooksBuilder(), _selectBooksTemplate),
                Query(BookFilesBuilder(), _selectBookFilesTemplate));
        }

        public List<BookStatistics> BookStatistics(int bookId)
        {
            return MapResults(Query(BooksBuilder().Where<Book>(x => x.Id == bookId), _selectBooksTemplate),
                Query(BookFilesBuilder().Where<BookFile>(x => x.BookId == bookId), _selectBookFilesTemplate));
        }

        private static List<BookStatistics> MapResults(List<BookStatistics> booksResult, List<BookStatistics> filesResult)
        {
            booksResult.ForEach(e =>
            {
                var file = filesResult.SingleOrDefault(f => f.BookId == e.BookId);

                e.SizeOnDisk = file?.SizeOnDisk ?? 0;
                e.ReleaseGroupsString = file?.ReleaseGroupsString;
            });

            return booksResult;
        }

        private List<BookStatistics> Query(SqlBuilder builder, string template)
        {
            var sql = builder.AddTemplate(template).LogQuery();

            using var conn = _database.OpenConnection();

            return conn.Query<BookStatistics>(sql.RawSql, sql.Parameters).ToList();
        }

        private SqlBuilder BooksBuilder()
        {
            return new SqlBuilder(_database.DatabaseType)
                .Select(@"""Books"".""Id"" AS BookId,
                        COUNT(""BookFiles"".""Id"") AS BookFileCount")
                .LeftJoin<Book, BookFile>((b, bf) => b.Id == bf.BookId)
                .GroupBy<Book>(x => x.Id);
        }

        private SqlBuilder BookFilesBuilder()
        {
            if (_database.DatabaseType == DatabaseType.SQLite)
            {
                return new SqlBuilder(_database.DatabaseType)
                    .Select(@"""BookId"",
                            SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                            GROUP_CONCAT(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                    .GroupBy<BookFile>(x => x.BookId);
            }

            return new SqlBuilder(_database.DatabaseType)
                .Select(@"""BookId"",
                        SUM(COALESCE(""Size"", 0)) AS SizeOnDisk,
                        string_agg(""ReleaseGroup"", '|') AS ReleaseGroupsString")
                .GroupBy<BookFile>(x => x.BookId);
        }
    }
}
