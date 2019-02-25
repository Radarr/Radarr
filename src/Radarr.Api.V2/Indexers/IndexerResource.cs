using NzbDrone.Core.Indexers;

namespace Radarr.Api.V2.Indexers
{
    public class IndexerResource : ProviderResource
    {
        public bool EnableRss { get; set; }
        public bool EnableSearch { get; set; }
        public bool SupportsRss { get; set; }
        public bool SupportsSearch { get; set; }
        public DownloadProtocol Protocol { get; set; }
    }

    public class IndexerResourceMapper : ProviderResourceMapper<IndexerResource, IndexerDefinition>
    {
        public override IndexerResource ToResource(IndexerDefinition definition)
        {
            if (definition == null) return null;

            var resource = base.ToResource(definition);

            resource.EnableRss = definition.EnableRss;
            resource.EnableSearch = definition.EnableSearch;
            resource.SupportsRss = definition.SupportsRss;
            resource.SupportsSearch = definition.SupportsSearch;
            resource.Protocol = definition.Protocol;

            return resource;
        }

        public override IndexerDefinition ToModel(IndexerResource resource)
        {
            if (resource == null) return null;

            var definition = base.ToModel(resource);

            definition.EnableRss = resource.EnableRss;
            definition.EnableSearch = resource.EnableSearch;

            return definition;
        }
    }
}
