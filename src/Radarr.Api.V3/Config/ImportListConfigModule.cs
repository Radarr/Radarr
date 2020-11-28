using NzbDrone.Core.Configuration;
using Radarr.Http.Validation;

namespace Radarr.Api.V3.Config
{
    public class ImportListConfigModule : RadarrConfigModule<ImportListConfigResource>
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
