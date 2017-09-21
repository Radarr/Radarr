using System.Linq;
using System.Reflection;
using NzbDrone.Core.Configuration;
using Lidarr.Http;

namespace Lidarr.Api.V3.Config
{
    public class MetadataProviderConfigModule : SonarrConfigModule<MetadataProviderConfigResource>
    {
        public MetadataProviderConfigModule(IConfigService configService)
            : base(configService)
        {

        }

        protected override MetadataProviderConfigResource ToResource(IConfigService model)
        {
            return MetadataProviderConfigResourceMapper.ToResource(model);
        }
    }
}
