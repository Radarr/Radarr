using FluentValidation;
using Radarr.Http.Validation;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class NetImportConfigModule : NzbDroneConfigModule<NetImportConfigResource>
    {

        public NetImportConfigModule(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.NetImportSyncInterval)
               .IsValidNetImportSyncInterval();
        }

        protected override NetImportConfigResource ToResource(IConfigService model)
        {
            return NetImportConfigResourceMapper.ToResource(model);
        }
    }
}
