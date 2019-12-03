using NzbDrone.Core.Configuration;

namespace Radarr.Api.V3.Config
{
    public class UiConfigModule : RadarrConfigModule<UiConfigResource>
    {
        public UiConfigModule(IConfigService configService)
            : base(configService)
        {

        }

        protected override UiConfigResource ToResource(IConfigService model)
        {
            return UiConfigResourceMapper.ToResource(model);
        }
    }
}
