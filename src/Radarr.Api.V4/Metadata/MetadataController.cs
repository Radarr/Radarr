using NzbDrone.Core.Extras.Metadata;
using Radarr.Http;

namespace Radarr.Api.V4.Metadata
{
    [V4ApiController]
    public class MetadataController : ProviderControllerBase<MetadataResource, IMetadata, MetadataDefinition>
    {
        public static readonly MetadataResourceMapper ResourceMapper = new MetadataResourceMapper();

        public MetadataController(IMetadataFactory metadataFactory)
            : base(metadataFactory, "metadata", ResourceMapper)
        {
        }
    }
}
