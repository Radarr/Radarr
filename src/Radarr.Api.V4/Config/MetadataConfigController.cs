using NzbDrone.Core.Configuration;
using Radarr.Http;

namespace Radarr.Api.V4.Config
{
    [V4ApiController("config/metadata")]
    public class MetadataConfigController : ConfigController<MetadataConfigResource>
    {
        public MetadataConfigController(IConfigService configService)
            : base(configService)
        {
        }

        protected override MetadataConfigResource ToResource(IConfigService model)
        {
            return MetadataConfigResourceMapper.ToResource(model);
        }
    }
}
