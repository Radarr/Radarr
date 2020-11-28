using NzbDrone.Core.Configuration;
using Radarr.Http.Validation;

namespace NzbDrone.Api.Config
{
    public class NetImportConfigModule : NzbDroneConfigModule<NetImportConfigResource>
    {
        public NetImportConfigModule(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.ImportListSyncInterval)
               .IsValidImportListSyncInterval();
        }

        protected override NetImportConfigResource ToResource(IConfigService model)
        {
            return NetImportConfigResourceMapper.ToResource(model);
        }
    }
}
