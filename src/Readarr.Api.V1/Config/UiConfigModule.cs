using NzbDrone.Core.Configuration;

namespace Readarr.Api.V1.Config
{
    public class UiConfigModule : ReadarrConfigModule<UiConfigResource>
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
