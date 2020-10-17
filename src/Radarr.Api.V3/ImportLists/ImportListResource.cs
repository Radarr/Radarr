using NzbDrone.Core.ImportLists;
using NzbDrone.Core.Movies;

namespace Radarr.Api.V3.ImportLists
{
    public class ImportListResource : ProviderResource
    {
        public bool Enabled { get; set; }
        public ImportListType EnableAuto { get; set; }
        public bool ShouldMonitor { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public bool SearchOnAdd { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public ImportListSource ListType { get; set; }
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

            resource.Enabled = definition.Enabled;
            resource.EnableAuto = definition.EnableAuto;
            resource.ShouldMonitor = definition.ShouldMonitor;
            resource.SearchOnAdd = definition.SearchOnAdd;
            resource.RootFolderPath = definition.RootFolderPath;
            resource.QualityProfileId = definition.ProfileId;
            resource.MinimumAvailability = definition.MinimumAvailability;
            resource.ListType = definition.ListType;
            resource.ListOrder = (int)definition.ListType;

            return resource;
        }

        public override ImportListDefinition ToModel(ImportListResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            var definition = base.ToModel(resource);

            definition.Enabled = resource.Enabled;
            definition.EnableAuto = resource.EnableAuto;
            definition.ShouldMonitor = resource.ShouldMonitor;
            definition.SearchOnAdd = resource.SearchOnAdd;
            definition.RootFolderPath = resource.RootFolderPath;
            definition.ProfileId = resource.QualityProfileId;
            definition.MinimumAvailability = resource.MinimumAvailability;
            definition.ListType = resource.ListType;

            return definition;
        }
    }
}
