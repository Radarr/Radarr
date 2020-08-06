using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.NetImport.ListMovies
{
    public interface IListMovieRepository : IBasicRepository<ListMovie>
    {
        List<ListMovie> GetAllForList(int listId);
    }

    public class ListMovieRepository : BasicRepository<ListMovie>, IListMovieRepository
    {
        public ListMovieRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<ListMovie> GetAllForList(int listId)
        {
            return Query(x => x.ListId == listId);
        }
    }
}
