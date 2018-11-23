using FluentValidation;
using Radarr.Http.ClientSchema;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.NetImport
{
    public class NetImportModule : ProviderModuleBase<NetImportResource, INetImport, NetImportDefinition>
    {
        public NetImportModule(NetImportFactory netImportFactory) : base(netImportFactory, "netimport")
        {
            PostValidator.RuleFor(c => c.RootFolderPath).IsValidPath();
            PostValidator.RuleFor(c => c.MinimumAvailability).NotNull();
            PostValidator.RuleFor(c => c.ProfileId).NotNull();
        }

        protected override void MapToResource(NetImportResource resource, NetImportDefinition definition)
        {
            base.MapToResource(resource, definition);

            resource.Enabled = definition.Enabled;
            resource.EnableAuto = definition.EnableAuto;
            resource.ProfileId = definition.ProfileId;
            resource.RootFolderPath = definition.RootFolderPath;
            resource.ShouldMonitor = definition.ShouldMonitor;
            resource.MinimumAvailability = definition.MinimumAvailability;
        }

        protected override void MapToModel(NetImportDefinition definition, NetImportResource resource)
        {
            base.MapToModel(definition, resource);

            definition.Enabled = resource.Enabled;
            definition.EnableAuto = resource.EnableAuto;
            definition.ProfileId = resource.ProfileId;
            definition.RootFolderPath = resource.RootFolderPath;
            definition.ShouldMonitor = resource.ShouldMonitor;
            definition.MinimumAvailability = resource.MinimumAvailability;
        }

        protected override void Validate(NetImportDefinition definition, bool includeWarnings)
        {
            if (!definition.Enable) return;
            base.Validate(definition, includeWarnings);
        }
    }
}
