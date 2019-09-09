using NzbDrone.Core.ImportLists;

namespace Lidarr.Api.V1.ImportLists
{
    public class ImportListResource : ProviderResource
    {
        public bool EnableAutomaticAdd { get; set; }
        public ImportListMonitorType ShouldMonitor { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public int MetadataProfileId { get; set; }
        public ImportListType ListType { get; set; }
        public int ListOrder { get; set; }
    }

    public class ImportListResourceMapper : ProviderResourceMapper<ImportListResource, ImportListDefinition>
    {
        public override ImportListResource ToResource(ImportListDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            var resource = base.ToResource(definition);
            
            resource.EnableAutomaticAdd = definition.EnableAutomaticAdd;
            resource.ShouldMonitor = definition.ShouldMonitor;
            resource.RootFolderPath = definition.RootFolderPath;
            resource.QualityProfileId = definition.ProfileId;
            resource.MetadataProfileId = definition.MetadataProfileId;
            resource.ListType = definition.ListType;
            resource.ListOrder = (int) definition.ListType;

            return resource;
        }

        public override ImportListDefinition ToModel(ImportListResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            var definition = base.ToModel(resource);
            
            definition.EnableAutomaticAdd = resource.EnableAutomaticAdd;
            definition.ShouldMonitor = resource.ShouldMonitor;
            definition.RootFolderPath = resource.RootFolderPath;
            definition.ProfileId = resource.QualityProfileId;
            definition.MetadataProfileId = resource.MetadataProfileId;
            definition.ListType = resource.ListType;

            return definition;
        }
    }
}
