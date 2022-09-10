using NzbDrone.Core.Indexers;
using Radarr.Http;

namespace Radarr.Api.V4.Indexers
{
    [V4ApiController]
    public class IndexerController : ProviderControllerBase<IndexerResource, IndexerBulkResource, IIndexer, IndexerDefinition>
    {
        public static readonly IndexerResourceMapper ResourceMapper = new IndexerResourceMapper();
        public static readonly IndexerBulkResourceMapper BulkResourceMapper = new IndexerBulkResourceMapper();

        public IndexerController(IndexerFactory indexerFactory)
            : base(indexerFactory, "indexer", ResourceMapper, BulkResourceMapper)
        {
        }
    }
}
