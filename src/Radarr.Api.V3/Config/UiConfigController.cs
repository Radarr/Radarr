using NzbDrone.Core.Configuration;
using Radarr.Http;

namespace Radarr.Api.V3.Config
{
    [V3ApiController("config/ui")]
    public class UiConfigController : ConfigController<UiConfigResource>
    {
        public UiConfigController(IConfigService configService)
            : base(configService)
        {
        }

        protected override UiConfigResource ToResource(IConfigService model)
        {
            return UiConfigResourceMapper.ToResource(model);
        }
    }
}
