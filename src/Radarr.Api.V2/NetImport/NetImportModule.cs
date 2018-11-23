using FluentValidation;
using Radarr.Http.ClientSchema;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Validation.Paths;

namespace Radarr.Api.V2.NetImport
{
    public class NetImportModule : ProviderModuleBase<NetImportResource, INetImport, NetImportDefinition>
    {
        public static readonly NetImportResourceMapper ResourceMapper = new NetImportResourceMapper();

        public NetImportModule(NetImportFactory netImportFactory)
            : base(netImportFactory, "netimport", ResourceMapper)
        {
            SharedValidator.RuleFor(c => c.RootFolderPath).IsValidPath();
            SharedValidator.RuleFor(c => c.MinimumAvailability).NotNull();
            SharedValidator.RuleFor(c => c.QualityProfileId).NotNull();
        }

        protected override void Validate(NetImportDefinition definition, bool includeWarnings)
        {
            if (!definition.Enable) return;
            base.Validate(definition, includeWarnings);
        }
    }
}
