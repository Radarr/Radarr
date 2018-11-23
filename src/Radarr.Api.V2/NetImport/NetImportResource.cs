using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport;

namespace Radarr.Api.V2.NetImport
{
    public class NetImportResource : ProviderResource
    {
        public bool Enabled { get; set; }
        public bool EnableAuto { get; set; }
        public bool ShouldMonitor { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
    }

    public class NetImportResourceMapper : ProviderResourceMapper<NetImportResource, NetImportDefinition>
    {
        public override NetImportResource ToResource(NetImportDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            var resource = base.ToResource(definition);

            resource.Enabled = definition.Enabled;
            resource.EnableAuto = definition.EnableAuto;
            resource.ShouldMonitor = definition.ShouldMonitor;
            resource.RootFolderPath = definition.RootFolderPath;
            resource.QualityProfileId = definition.ProfileId;

            return resource;
        }

        public override NetImportDefinition ToModel(NetImportResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            var definition = base.ToModel(resource);

            definition.Enabled = resource.Enabled;
            definition.EnableAuto = resource.EnableAuto;
            definition.ShouldMonitor = resource.ShouldMonitor;
            definition.RootFolderPath = resource.RootFolderPath;
            definition.ProfileId = resource.QualityProfileId;

            return definition;
        }
    }
}
