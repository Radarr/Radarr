using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public interface IBookRepository : IBasicRepository<Book>
    {
        bool BookPathExists(string path);
        Book FindByIsbn(string isbn);
        Book FindByIsbn13(string isbn13);
        Book FindByAsin(string asin);
        Book FindByForeignId(string foreignBookId);
        List<Book> FindByAuthorId(int authorId);
        List<Book> FindBySeriesId(int seriesId);
        List<Book> BooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        Book FindByPath(string path);
        Dictionary<int, string> AllBookPaths();
        Dictionary<int, List<int>> AllBookTags();
    }

    public class BookRepository : BasicRepository<Book>, IBookRepository
    {
        public BookRepository(IMainDatabase database,
                              IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool BookPathExists(string path)
        {
            return Query(b => b.Path == path).Any();
        }

        public Book FindByIsbn(string isbn)
        {
            return Query(b => b.Isbn == isbn).FirstOrDefault();
        }

        public Book FindByIsbn13(string isbn13)
        {
            return Query(b => b.Isbn13 == isbn13).FirstOrDefault();
        }

        public Book FindByAsin(string asin)
        {
            return Query(b => b.Asin == asin).FirstOrDefault();
        }

        public Book FindByForeignId(string foreignBookId)
        {
            return Query(b => b.ForeignBookId == foreignBookId).FirstOrDefault();
        }

        public List<Book> FindByAuthorId(int authorId)
        {
            return Query(b => b.AuthorId == authorId);
        }

        public List<Book> FindBySeriesId(int seriesId)
        {
            return Query(b => b.SeriesId == seriesId);
        }

        public List<Book> BooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var query = Query(b => b.ReleaseDate >= start && b.ReleaseDate <= end);

            if (!includeUnmonitored)
            {
                query = query.Where(b => b.Monitored).ToList();
            }

            return query;
        }

        public Book FindByPath(string path)
        {
            return Query(b => b.Path == path).FirstOrDefault();
        }

        public Dictionary<int, string> AllBookPaths()
        {
            var books = All();
            return books.ToDictionary(b => b.Id, b => b.Path);
        }

        public Dictionary<int, List<int>> AllBookTags()
        {
            var books = All();
            return books.ToDictionary(b => b.Id, b => b.Tags.ToList());
        }
    }
}
