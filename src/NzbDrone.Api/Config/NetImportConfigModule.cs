using NzbDrone.Core.Configuration;
using Radarr.Http.Validation;

namespace NzbDrone.Api.Config
{
    public class ImportListConfigModule : NzbDroneConfigModule<ImportListConfigResource>
    {
        public ImportListConfigModule(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.ImportListSyncInterval)
               .IsValidImportListSyncInterval();
        }

        protected override ImportListConfigResource ToResource(IConfigService model)
        {
            return ImportListConfigResourceMapper.ToResource(model);
        }
    }
}
