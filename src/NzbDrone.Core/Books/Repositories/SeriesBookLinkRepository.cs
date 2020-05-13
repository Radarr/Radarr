using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public interface ISeriesBookLinkRepository : IBasicRepository<SeriesBookLink>
    {
        List<SeriesBookLink> GetLinksBySeries(int seriesId);
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
    }
}
