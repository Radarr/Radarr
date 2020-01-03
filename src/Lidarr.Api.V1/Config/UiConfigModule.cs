using NzbDrone.Core.Configuration;

namespace Lidarr.Api.V1.Config
{
    public class UiConfigModule : LidarrConfigModule<UiConfigResource>
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
