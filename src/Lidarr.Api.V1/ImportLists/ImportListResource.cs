using NzbDrone.Core.ImportLists;

namespace Lidarr.Api.V1.ImportLists
{
    public class ImportListResource : ProviderResource
    {
        public bool EnableAutomaticAdd { get; set; }
        public bool ShouldMonitor { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public int LanguageProfileId { get; set; }
        public int MetadataProfileId { get; set; }
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
            resource.LanguageProfileId = definition.LanguageProfileId;
            resource.MetadataProfileId = definition.MetadataProfileId;

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
            definition.LanguageProfileId = resource.LanguageProfileId;
            definition.MetadataProfileId = resource.MetadataProfileId;

            return definition;
        }
    }
}
