using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatRepository : IBasicRepository<CustomFormatDefinition>
    {

    }

    public class CustomFormatRepository : BasicRepository<CustomFormatDefinition>, ICustomFormatRepository
    {
        public CustomFormatRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
