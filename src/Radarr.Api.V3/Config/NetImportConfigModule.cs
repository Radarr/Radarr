using NzbDrone.Core.Configuration;
using Radarr.Http.Validation;

namespace Radarr.Api.V3.Config
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
