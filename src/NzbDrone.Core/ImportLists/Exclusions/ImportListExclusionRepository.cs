using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.Exclusions
{
    public interface IImportListExclusionRepository : IBasicRepository<ImportListExclusion>
    {
        ImportListExclusion FindByForeignId(string foreignId);
        List<ImportListExclusion> FindByForeignId(List<string> ids);
    }

    public class ImportListExclusionRepository : BasicRepository<ImportListExclusion>, IImportListExclusionRepository
    {
        public ImportListExclusionRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public ImportListExclusion FindByForeignId(string foreignId)
        {
            return Query(m => m.ForeignId == foreignId).SingleOrDefault();
        }

        public List<ImportListExclusion> FindByForeignId(List<string> ids)
        {
            // Using Enumerable.Contains forces the builder to create an 'IN'
            // and not a string 'LIKE' expression
            return Query(x => Enumerable.Contains(ids, x.ForeignId));
        }
    }
}
