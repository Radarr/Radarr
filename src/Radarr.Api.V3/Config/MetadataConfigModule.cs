using NzbDrone.Core.Configuration;

namespace Radarr.Api.V3.Config
{
    public class MetadataConfigModule : RadarrConfigModule<MetadataConfigResource>
    {
        public MetadataConfigModule(IConfigService configService)
            : base(configService)
        {
        }

        protected override MetadataConfigResource ToResource(IConfigService model)
        {
            return MetadataConfigResourceMapper.ToResource(model);
        }
    }
}
