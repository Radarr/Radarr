using NzbDrone.Core.Configuration;
using Radarr.Http;
using Radarr.Http.Validation;

namespace Radarr.Api.V4.Config
{
    [V4ApiController("config/importlist")]

    public class ImportListConfigController : ConfigController<ImportListConfigResource>
    {
        public ImportListConfigController(IConfigService configService)
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
