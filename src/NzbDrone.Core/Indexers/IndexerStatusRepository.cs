﻿using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider;


namespace NzbDrone.Core.Indexers
{
    public interface IIndexerStatusRepository : IProviderRepository<IndexerStatus>
    {
        IndexerStatus FindByIndexerId(int indexerId);
     }

    public class IndexerStatusRepository : ProviderRepository<IndexerStatus>, IIndexerStatusRepository
    {
        public IndexerStatusRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public IndexerStatus FindByIndexerId(int indexerId)
        {
            return Query(q => q.Where(c => c.IndexerId == indexerId).SingleOrDefault());
        }
    }
}
