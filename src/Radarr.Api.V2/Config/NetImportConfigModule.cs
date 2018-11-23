using FluentValidation;
using Radarr.Http.Validation;
using NzbDrone.Core.Configuration;

namespace Radarr.Api.V2.Config
{
    public class NetImportConfigModule : RadarrConfigModule<NetImportConfigResource>
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
