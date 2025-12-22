using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Series
{
    public interface ISeriesRepository : IBasicRepository<Series>
    {
        Series FindByTitle(string title);
        Series FindByForeignId(string foreignSeriesId);
        List<Series> FindByAuthorId(int authorId);
        List<Series> GetMonitored();
    }

    public class SeriesRepository : BasicRepository<Series>, ISeriesRepository
    {
        public SeriesRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public Series FindByTitle(string title)
        {
            return Query(s => s.Title == title).FirstOrDefault();
        }

        public Series FindByForeignId(string foreignSeriesId)
        {
            return Query(s => s.ForeignSeriesId == foreignSeriesId).FirstOrDefault();
        }

        public List<Series> FindByAuthorId(int authorId)
        {
            return Query(s => s.AuthorId == authorId);
        }

        public List<Series> GetMonitored()
        {
            return Query(s => s.Monitored);
        }
    }
}
