using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public interface ISeriesBookLinkRepository : IBasicRepository<SeriesBookLink>
    {
        List<SeriesBookLink> GetLinksBySeries(int seriesId);
        List<SeriesBookLink> GetLinksByBook(List<int> bookIds);
    }

    public class SeriesBookLinkRepository : BasicRepository<SeriesBookLink>, ISeriesBookLinkRepository
    {
        public SeriesBookLinkRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<SeriesBookLink> GetLinksBySeries(int seriesId)
        {
            return Query(x => x.SeriesId == seriesId);
        }

        public List<SeriesBookLink> GetLinksByBook(List<int> bookIds)
        {
            return _database.QueryJoined<SeriesBookLink, Series>(
                Builder()
                .Join<SeriesBookLink, Series>((l, s) => l.SeriesId == s.Id)
                .Where<SeriesBookLink>(x => bookIds.Contains(x.BookId)),
                (link, series) =>
                {
                    link.Series = series;
                    return link;
                })
                .ToList();
        }
    }
}
