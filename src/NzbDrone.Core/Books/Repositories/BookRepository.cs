using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Books
{
    public interface IBookRepository : IBasicRepository<Book>
    {
        List<Book> GetBooks(int authorId);
        List<Book> GetLastBooks(IEnumerable<int> authorMetadataIds);
        List<Book> GetNextBooks(IEnumerable<int> authorMetadataIds);
        List<Book> GetBooksByAuthorMetadataId(int authorMetadataId);
        List<Book> GetBooksForRefresh(int authorMetadataId, IEnumerable<string> foreignIds);
        List<Book> GetBooksByFileIds(IEnumerable<int> fileIds);
        Book FindByTitle(int authorMetadataId, string title);
        Book FindById(string foreignBookId);
        Book FindBySlug(string titleSlug);
        PagingSpec<Book> BooksWithoutFiles(PagingSpec<Book> pagingSpec);
        PagingSpec<Book> BooksWhereCutoffUnmet(PagingSpec<Book> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
        List<Book> BooksBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored);
        List<Book> AuthorBooksBetweenDates(Author author, DateTime startDate, DateTime endDate, bool includeUnmonitored);
        void SetMonitoredFlat(Book book, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        List<Book> GetArtistAlbumsWithFiles(Author author);
    }

    public class BookRepository : BasicRepository<Book>, IBookRepository
    {
        public BookRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Book> GetBooks(int authorId)
        {
            return Query(Builder().Join<Book, Author>((l, r) => l.AuthorMetadataId == r.AuthorMetadataId).Where<Author>(a => a.Id == authorId));
        }

        public List<Book> GetLastBooks(IEnumerable<int> authorMetadataIds)
        {
            var now = DateTime.UtcNow;
            return Query(Builder().Where<Book>(x => authorMetadataIds.Contains(x.AuthorMetadataId) && x.ReleaseDate < now)
                         .GroupBy<Book>(x => x.AuthorMetadataId)
                         .Having("Books.ReleaseDate = MAX(Books.ReleaseDate)"));
        }

        public List<Book> GetNextBooks(IEnumerable<int> authorMetadataIds)
        {
            var now = DateTime.UtcNow;
            return Query(Builder().Where<Book>(x => authorMetadataIds.Contains(x.AuthorMetadataId) && x.ReleaseDate > now)
                         .GroupBy<Book>(x => x.AuthorMetadataId)
                         .Having("Books.ReleaseDate = MIN(Books.ReleaseDate)"));
        }

        public List<Book> GetBooksByAuthorMetadataId(int authorMetadataId)
        {
            return Query(s => s.AuthorMetadataId == authorMetadataId);
        }

        public List<Book> GetBooksForRefresh(int authorMetadataId, IEnumerable<string> foreignIds)
        {
            return Query(a => a.AuthorMetadataId == authorMetadataId || foreignIds.Contains(a.ForeignBookId));
        }

        public List<Book> GetBooksByFileIds(IEnumerable<int> fileIds)
        {
            return Query(new SqlBuilder()
                         .Join<Book, BookFile>((l, r) => l.Id == r.BookId)
                         .Where<BookFile>(f => fileIds.Contains(f.Id)))
                .DistinctBy(x => x.Id)
                .ToList();
        }

        public Book FindById(string foreignBookId)
        {
            return Query(s => s.ForeignBookId == foreignBookId).SingleOrDefault();
        }

        public Book FindBySlug(string titleSlug)
        {
            return Query(s => s.TitleSlug == titleSlug).SingleOrDefault();
        }

        //x.Id == null is converted to SQL, so warning incorrect
#pragma warning disable CS0472
        private SqlBuilder AlbumsWithoutFilesBuilder(DateTime currentTime) => Builder()
            .Join<Book, Author>((l, r) => l.AuthorMetadataId == r.AuthorMetadataId)
            .LeftJoin<Book, BookFile>((t, f) => t.Id == f.BookId)
            .Where<BookFile>(f => f.Id == null)
            .Where<Book>(a => a.ReleaseDate <= currentTime);
#pragma warning restore CS0472

        public PagingSpec<Book> BooksWithoutFiles(PagingSpec<Book> pagingSpec)
        {
            var currentTime = DateTime.UtcNow;

            pagingSpec.Records = GetPagedRecords(AlbumsWithoutFilesBuilder(currentTime), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(AlbumsWithoutFilesBuilder(currentTime).SelectCountDistinct<Book>(x => x.Id), pagingSpec);

            return pagingSpec;
        }

        private SqlBuilder AlbumsWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff) => Builder()
            .Join<Book, Author>((l, r) => l.AuthorMetadataId == r.AuthorMetadataId)
            .Join<Book, BookFile>((t, f) => t.Id == f.BookId)
            .Where(BuildQualityCutoffWhereClause(qualitiesBelowCutoff));

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("(Authors.[QualityProfileId] = {0} AND BookFiles.Quality LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        public PagingSpec<Book> BooksWhereCutoffUnmet(PagingSpec<Book> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            pagingSpec.Records = GetPagedRecords(AlbumsWhereCutoffUnmetBuilder(qualitiesBelowCutoff), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM {TableMapping.Mapper.TableNameMapping(typeof(Book))} /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/)";
            pagingSpec.TotalRecords = GetPagedRecordCount(AlbumsWhereCutoffUnmetBuilder(qualitiesBelowCutoff).Select(typeof(Book)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        public List<Book> BooksBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var builder = Builder().Where<Book>(rg => rg.ReleaseDate >= startDate && rg.ReleaseDate <= endDate);

            if (!includeUnmonitored)
            {
                builder = builder.Where<Book>(e => e.Monitored == true)
                    .Join<Book, Author>((l, r) => l.AuthorMetadataId == r.AuthorMetadataId)
                    .Where<Author>(e => e.Monitored == true);
            }

            return Query(builder);
        }

        public List<Book> AuthorBooksBetweenDates(Author author, DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var builder = Builder().Where<Book>(rg => rg.ReleaseDate >= startDate &&
                                                 rg.ReleaseDate <= endDate &&
                                                 rg.AuthorMetadataId == author.AuthorMetadataId);

            if (!includeUnmonitored)
            {
                builder = builder.Where<Book>(e => e.Monitored == true)
                    .Join<Book, Author>((l, r) => l.AuthorMetadataId == r.AuthorMetadataId)
                    .Where<Author>(e => e.Monitored == true);
            }

            return Query(builder);
        }

        public void SetMonitoredFlat(Book book, bool monitored)
        {
            book.Monitored = monitored;
            SetFields(book, p => p.Monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            var albums = ids.Select(x => new Book { Id = x, Monitored = monitored }).ToList();
            SetFields(albums, p => p.Monitored);
        }

        public Book FindByTitle(int authorMetadataId, string title)
        {
            var cleanTitle = Parser.Parser.CleanAuthorName(title);

            if (string.IsNullOrEmpty(cleanTitle))
            {
                cleanTitle = title;
            }

            return Query(s => (s.CleanTitle == cleanTitle || s.Title == title) && s.AuthorMetadataId == authorMetadataId)
                .ExclusiveOrDefault();
        }

        public List<Book> GetArtistAlbumsWithFiles(Author author)
        {
            return Query(Builder()
                         .Join<Book, BookFile>((t, f) => t.Id == f.BookId)
                         .Where<Book>(x => x.AuthorMetadataId == author.AuthorMetadataId));
        }
    }
}
