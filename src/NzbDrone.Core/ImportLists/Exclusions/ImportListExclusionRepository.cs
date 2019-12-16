using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using System.Collections.Generic;
using System.Linq;

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
            return Query.Where<ImportListExclusion>(m => m.ForeignId == foreignId).SingleOrDefault();
        }

        public List<ImportListExclusion> FindByForeignId(List<string> ids)
        {
            return Query.Where($"[ForeignId] IN ('{string.Join("', '", ids)}')").ToList();
        }
    }
}
