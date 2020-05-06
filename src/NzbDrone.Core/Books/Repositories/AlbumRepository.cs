using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Music
{
    public interface IAlbumRepository : IBasicRepository<Book>
    {
        List<Book> GetAlbums(int authorId);
        List<Book> GetLastAlbums(IEnumerable<int> artistMetadataIds);
        List<Book> GetNextAlbums(IEnumerable<int> artistMetadataIds);
        List<Book> GetAlbumsByArtistMetadataId(int artistMetadataId);
        List<Book> GetAlbumsForRefresh(int artistMetadataId, IEnumerable<string> foreignIds);
        List<Book> GetAlbumsByFileIds(IEnumerable<int> fileIds);
        Book FindByTitle(int artistMetadataId, string title);
        Book FindById(string foreignBookId);
        Book FindBySlug(string titleSlug);
        PagingSpec<Book> AlbumsWithoutFiles(PagingSpec<Book> pagingSpec);
        PagingSpec<Book> AlbumsWhereCutoffUnmet(PagingSpec<Book> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
        List<Book> AlbumsBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored);
        List<Book> ArtistAlbumsBetweenDates(Author artist, DateTime startDate, DateTime endDate, bool includeUnmonitored);
        void SetMonitoredFlat(Book album, bool monitored);
        void SetMonitored(IEnumerable<int> ids, bool monitored);
        List<Book> GetArtistAlbumsWithFiles(Author artist);
    }

    public class AlbumRepository : BasicRepository<Book>, IAlbumRepository
    {
        public AlbumRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<Book> GetAlbums(int authorId)
        {
            return Query(Builder().Join<Book, Author>((l, r) => l.AuthorMetadataId == r.AuthorMetadataId).Where<Author>(a => a.Id == authorId));
        }

        public List<Book> GetLastAlbums(IEnumerable<int> artistMetadataIds)
        {
            var now = DateTime.UtcNow;
            return Query(Builder().Where<Book>(x => artistMetadataIds.Contains(x.AuthorMetadataId) && x.ReleaseDate < now)
                         .GroupBy<Book>(x => x.AuthorMetadataId)
                         .Having("Books.ReleaseDate = MAX(Books.ReleaseDate)"));
        }

        public List<Book> GetNextAlbums(IEnumerable<int> artistMetadataIds)
        {
            var now = DateTime.UtcNow;
            return Query(Builder().Where<Book>(x => artistMetadataIds.Contains(x.AuthorMetadataId) && x.ReleaseDate > now)
                         .GroupBy<Book>(x => x.AuthorMetadataId)
                         .Having("Books.ReleaseDate = MIN(Books.ReleaseDate)"));
        }

        public List<Book> GetAlbumsByArtistMetadataId(int artistMetadataId)
        {
            return Query(s => s.AuthorMetadataId == artistMetadataId);
        }

        public List<Book> GetAlbumsForRefresh(int artistMetadataId, IEnumerable<string> foreignIds)
        {
            return Query(a => a.AuthorMetadataId == artistMetadataId || foreignIds.Contains(a.ForeignBookId));
        }

        public List<Book> GetAlbumsByFileIds(IEnumerable<int> fileIds)
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

        public PagingSpec<Book> AlbumsWithoutFiles(PagingSpec<Book> pagingSpec)
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

        public PagingSpec<Book> AlbumsWhereCutoffUnmet(PagingSpec<Book> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            pagingSpec.Records = GetPagedRecords(AlbumsWhereCutoffUnmetBuilder(qualitiesBelowCutoff), pagingSpec, PagedQuery);

            var countTemplate = $"SELECT COUNT(*) FROM (SELECT /**select**/ FROM {TableMapping.Mapper.TableNameMapping(typeof(Book))} /**join**/ /**innerjoin**/ /**leftjoin**/ /**where**/ /**groupby**/ /**having**/)";
            pagingSpec.TotalRecords = GetPagedRecordCount(AlbumsWhereCutoffUnmetBuilder(qualitiesBelowCutoff).Select(typeof(Book)), pagingSpec, countTemplate);

            return pagingSpec;
        }

        public List<Book> AlbumsBetweenDates(DateTime startDate, DateTime endDate, bool includeUnmonitored)
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

        public List<Book> ArtistAlbumsBetweenDates(Author artist, DateTime startDate, DateTime endDate, bool includeUnmonitored)
        {
            var builder = Builder().Where<Book>(rg => rg.ReleaseDate >= startDate &&
                                                 rg.ReleaseDate <= endDate &&
                                                 rg.AuthorMetadataId == artist.AuthorMetadataId);

            if (!includeUnmonitored)
            {
                builder = builder.Where<Book>(e => e.Monitored == true)
                    .Join<Book, Author>((l, r) => l.AuthorMetadataId == r.AuthorMetadataId)
                    .Where<Author>(e => e.Monitored == true);
            }

            return Query(builder);
        }

        public void SetMonitoredFlat(Book album, bool monitored)
        {
            album.Monitored = monitored;
            SetFields(album, p => p.Monitored);
        }

        public void SetMonitored(IEnumerable<int> ids, bool monitored)
        {
            var albums = ids.Select(x => new Book { Id = x, Monitored = monitored }).ToList();
            SetFields(albums, p => p.Monitored);
        }

        public Book FindByTitle(int artistMetadataId, string title)
        {
            var cleanTitle = Parser.Parser.CleanArtistName(title);

            if (string.IsNullOrEmpty(cleanTitle))
            {
                cleanTitle = title;
            }

            return Query(s => (s.CleanTitle == cleanTitle || s.Title == title) && s.AuthorMetadataId == artistMetadataId)
                .ExclusiveOrDefault();
        }

        public List<Book> GetArtistAlbumsWithFiles(Author artist)
        {
            return Query(Builder()
                         .Join<Book, BookFile>((t, f) => t.Id == f.BookId)
                         .Where<Book>(x => x.AuthorMetadataId == artist.AuthorMetadataId));
        }
    }
}
