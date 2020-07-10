using FluentValidation;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace Radarr.Api.V3.NetImport
{
    public class NetImportModule : ProviderModuleBase<NetImportResource, INetImport, NetImportDefinition>
    {
        public static readonly NetImportResourceMapper ResourceMapper = new NetImportResourceMapper();

        public NetImportModule(NetImportFactory netImportFactory, ProfileExistsValidator profileExistsValidator)
            : base(netImportFactory, "netimport", ResourceMapper)
        {
            SharedValidator.RuleFor(c => c.RootFolderPath).IsValidPath();
            SharedValidator.RuleFor(c => c.MinimumAvailability).NotNull();
            SharedValidator.RuleFor(c => c.QualityProfileId).ValidId();
            SharedValidator.RuleFor(c => c.QualityProfileId).SetValidator(profileExistsValidator);
        }

        protected override void Validate(NetImportDefinition definition, bool includeWarnings)
        {
            if (!definition.Enable)
            {
                return;
            }

            base.Validate(definition, includeWarnings);
        }
    }
}
