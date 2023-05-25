using NzbDrone.Core.Extras.Metadata;

namespace Radarr.Api.V3.Metadata
{
    public class MetadataBulkResource : ProviderBulkResource<MetadataBulkResource>
    {
    }

    public class MetadataBulkResourceMapper : ProviderBulkResourceMapper<MetadataBulkResource, MetadataDefinition>
    {
    }
}
