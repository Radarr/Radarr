using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<History>
    {
        History MostRecentForBook(int bookId);
        History MostRecentForDownloadId(string downloadId);
        List<History> FindByDownloadId(string downloadId);
        List<History> GetByAuthor(int authorId, HistoryEventType? eventType);
        List<History> GetByBook(int bookId, HistoryEventType? eventType);
        List<History> FindDownloadHistory(int idAuthorId, QualityModel quality);
        void DeleteForAuthor(int authorId);
        List<History> Since(DateTime date, HistoryEventType? eventType);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {
        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public History MostRecentForBook(int bookId)
        {
            return Query(h => h.BookId == bookId)
                .OrderByDescending(h => h.Date)
                .FirstOrDefault();
        }

        public History MostRecentForDownloadId(string downloadId)
        {
            return Query(h => h.DownloadId == downloadId)
                .OrderByDescending(h => h.Date)
                .FirstOrDefault();
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return _database.QueryJoined<History, Author, Book>(
                Builder()
                .Join<History, Author>((h, a) => h.AuthorId == a.Id)
                .Join<History, Book>((h, a) => h.BookId == a.Id)
                .Where<History>(h => h.DownloadId == downloadId),
                (history, author, book) =>
                {
                    history.Author = author;
                    history.Book = book;
                    return history;
                }).ToList();
        }

        public List<History> GetByAuthor(int authorId, HistoryEventType? eventType)
        {
            var builder = Builder().Where<History>(h => h.AuthorId == authorId);

            if (eventType.HasValue)
            {
                builder.Where<History>(h => h.EventType == eventType);
            }

            return Query(builder).OrderByDescending(h => h.Date).ToList();
        }

        public List<History> GetByBook(int bookId, HistoryEventType? eventType)
        {
            var builder = Builder()
                .Join<History, Book>((h, a) => h.BookId == a.Id)
                .Where<History>(h => h.BookId == bookId);

            if (eventType.HasValue)
            {
                builder.Where<History>(h => h.EventType == eventType);
            }

            return _database.QueryJoined<History, Book>(
                builder,
                (history, book) =>
                {
                    history.Book = book;
                    return history;
                }).OrderByDescending(h => h.Date).ToList();
        }

        public List<History> FindDownloadHistory(int idAuthorId, QualityModel quality)
        {
            var allowed = new[] { HistoryEventType.Grabbed, HistoryEventType.DownloadFailed, HistoryEventType.BookFileImported };

            return Query(h => h.AuthorId == idAuthorId &&
                         h.Quality == quality &&
                         allowed.Contains(h.EventType));
        }

        public void DeleteForAuthor(int authorId)
        {
            Delete(c => c.AuthorId == authorId);
        }

        protected override SqlBuilder PagedBuilder() => new SqlBuilder()
            .Join<History, Author>((h, a) => h.AuthorId == a.Id)
            .Join<History, Book>((h, a) => h.BookId == a.Id);

        protected override IEnumerable<History> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<History, Author, Book>(builder, (history, author, book) =>
                    {
                        history.Author = author;
                        history.Book = book;
                        return history;
                    });

        public List<History> Since(DateTime date, HistoryEventType? eventType)
        {
            var builder = Builder().Where<History>(x => x.Date >= date);

            if (eventType.HasValue)
            {
                builder.Where<History>(h => h.EventType == eventType);
            }

            return Query(builder).OrderBy(h => h.Date).ToList();
        }
    }
}
