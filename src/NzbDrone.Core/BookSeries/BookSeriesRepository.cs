using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.BookSeries
{
    public interface IBookSeriesRepository : IBasicRepository<BookSeries>
    {
        BookSeries FindByTitle(string title);
        BookSeries FindByForeignId(string foreignSeriesId);
        List<BookSeries> FindByAuthorId(int authorId);
        List<BookSeries> GetMonitored();
    }

    public class BookSeriesRepository : BasicRepository<BookSeries>, IBookSeriesRepository
    {
        public BookSeriesRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public BookSeries FindByTitle(string title)
        {
            return Query(s => s.Title == title).FirstOrDefault();
        }

        public BookSeries FindByForeignId(string foreignSeriesId)
        {
            return Query(s => s.ForeignSeriesId == foreignSeriesId).FirstOrDefault();
        }

        public List<BookSeries> FindByAuthorId(int authorId)
        {
            return Query(s => s.AuthorId == authorId);
        }

        public List<BookSeries> GetMonitored()
        {
            return Query(s => s.Monitored);
        }
    }
}
