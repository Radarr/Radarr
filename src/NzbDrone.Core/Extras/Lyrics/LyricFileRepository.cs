using NzbDrone.Core.Datastore;
using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Extras.Lyrics
{
    public interface ILyricFileRepository : IExtraFileRepository<LyricFile>
    {
    }

    public class LyricFileRepository : ExtraFileRepository<LyricFile>, ILyricFileRepository
    {
        public LyricFileRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }
    }
}
