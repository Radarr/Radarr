using NzbDrone.Core.Extras.Metadata;

namespace Lidarr.Api.V1.Metadata
{
    public class MetadataResource : ProviderResource
    {
        public bool Enable { get; set; }
    }

    public class MetadataResourceMapper : ProviderResourceMapper<MetadataResource, MetadataDefinition>
    {
        public override MetadataResource ToResource(MetadataDefinition definition)
        {
            if (definition == null) return null;

            var resource = base.ToResource(definition);

            resource.Enable = definition.Enable;

            return resource;
        }

        public override MetadataDefinition ToModel(MetadataResource resource)
        {
            if (resource == null) return null;

            var definition = base.ToModel(resource);

            definition.Enable = resource.Enable;

            return definition;
        }
    }
}