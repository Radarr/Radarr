using NzbDrone.Api.ClientSchema;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Api.NetImport
{
    public class NetImportModule : ProviderModuleBase<NetImportResource, INetImport, NetImportDefinition>
    {
        private readonly IProfileService _profileService;
        public NetImportModule(NetImportFactory indexerFactory, IProfileService profileService)
            : base(indexerFactory, "netimport")
        {
            _profileService = profileService;
        }

        protected override void MapToResource(NetImportResource resource, NetImportDefinition definition)
        {
            base.MapToResource(resource, definition);

            resource.Enabled = definition.Enabled;
            resource.EnableAuto = definition.EnableAuto;
            resource.ProfileId = definition.ProfileId;
            resource.RootFolderPath = definition.RootFolderPath;
            resource.ShouldMonitor = definition.ShouldMonitor;
        }

        protected override void MapToModel(NetImportDefinition definition, NetImportResource resource)
        {
            base.MapToModel(definition, resource);

            definition.Enabled = resource.Enabled;
            definition.EnableAuto = resource.EnableAuto;
            definition.ProfileId = resource.ProfileId;
            definition.RootFolderPath = resource.RootFolderPath;
            definition.ShouldMonitor = resource.ShouldMonitor;
        }

        protected override void Validate(NetImportDefinition definition, bool includeWarnings)
        {
            if (!definition.Enable) return;
            base.Validate(definition, includeWarnings);
        }
    }
}