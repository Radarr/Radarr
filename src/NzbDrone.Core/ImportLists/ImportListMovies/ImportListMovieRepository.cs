using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.ImportListMovies
{
    public interface IImportListMovieRepository : IBasicRepository<ImportListMovie>
    {
        List<ImportListMovie> GetAllForLists(List<int> listIds);
        bool ExistsByMetadataId(int metadataId);
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

        public bool ExistsByMetadataId(int metadataId)
        {
            return Query(x => x.MovieMetadataId == metadataId).Any();
        }
    }
}
