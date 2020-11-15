using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.ImportListMovies
{
    public interface IImportListMovieRepository : IBasicRepository<ImportListMovie>
    {
        List<ImportListMovie> GetAllForLists(List<int> listIds);
    }

    public class ImportListMovieRepository : BasicRepository<ImportListMovie>, IImportListMovieRepository
    {
        public ImportListMovieRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<ImportListMovie> GetAllForLists(List<int> listIds)
        {
            return Query(x => listIds.Contains(x.ListId));
        }
    }
}
