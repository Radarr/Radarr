using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Movies.AlternativeTitles
{
    public interface IAlternativeTitleRepository : IBasicRepository<AlternativeTitle>
    {
        
    }

    public class AlternativeTitleRepository : BasicRepository<AlternativeTitle>, IAlternativeTitleRepository
    {
		protected IMainDatabase _database;

        public AlternativeTitleRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
			_database = database;
        }
    }
}
