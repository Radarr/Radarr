using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public interface ISeriesRepository : IBasicRepository<Series>
    {
        Series FindById(string foreignSeriesId);
        List<Series> FindById(IEnumerable<string> foreignSeriesId);
        List<Series> GetByAuthorMetadataId(int authorMetadataId);
        List<Series> GetByAuthorId(int authorId);
    }

    public class SeriesRepository : BasicRepository<Series>, ISeriesRepository
    {
        public SeriesRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Series FindById(string foreignSeriesId)
        {
            return Query(x => x.ForeignSeriesId == foreignSeriesId).SingleOrDefault();
        }

        public List<Series> FindById(IEnumerable<string> foreignSeriesId)
        {
            return Query(x => foreignSeriesId.Contains(x.ForeignSeriesId));
        }

        public List<Series> GetByAuthorMetadataId(int authorMetadataId)
        {
            return QueryDistinct(Builder().Join<Series, SeriesBookLink>((l, r) => l.Id == r.SeriesId)
                                 .Join<SeriesBookLink, Book>((l, r) => l.BookId == r.Id)
                                 .Where<Book>(x => x.AuthorMetadataId == authorMetadataId));
        }

        public List<Series> GetByAuthorId(int authorId)
        {
            return QueryDistinct(Builder().Join<Series, SeriesBookLink>((l, r) => l.Id == r.SeriesId)
                                 .Join<SeriesBookLink, Book>((l, r) => l.BookId == r.Id)
                                 .Join<Book, Author>((l, r) => l.AuthorMetadataId == r.AuthorMetadataId)
                                 .Where<Author>(x => x.Id == authorId));
        }
    }
}
