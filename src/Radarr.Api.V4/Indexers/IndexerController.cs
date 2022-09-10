using NzbDrone.Core.Indexers;
using Radarr.Http;

namespace Radarr.Api.V4.Indexers
{
    [V4ApiController]
    public class IndexerController : ProviderControllerBase<IndexerResource, IIndexer, IndexerDefinition>
    {
        public static readonly IndexerResourceMapper ResourceMapper = new IndexerResourceMapper();

        public IndexerController(IndexerFactory indexerFactory)
            : base(indexerFactory, "indexer", ResourceMapper)
        {
        }
    }
}
