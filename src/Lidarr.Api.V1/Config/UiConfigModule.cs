using System.Linq;
using System.Reflection;
using NzbDrone.Core.Configuration;
using Lidarr.Http;

namespace Lidarr.Api.V1.Config
{
    public class UiConfigModule : SonarrConfigModule<UiConfigResource>
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