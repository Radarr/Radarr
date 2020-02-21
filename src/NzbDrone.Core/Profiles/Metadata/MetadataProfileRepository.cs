using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Profiles.Metadata
{
    public interface IMetadataProfileRepository : IBasicRepository<MetadataProfile>
    {
        bool Exists(int id);
    }

    public class MetadataProfileRepository : BasicRepository<MetadataProfile>, IMetadataProfileRepository
    {
        public MetadataProfileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool Exists(int id)
        {
            return Query(p => p.Id == id).Count == 1;
        }
    }
}
