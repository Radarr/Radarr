using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation.Paths;

namespace Radarr.Api.V2.Config
{
    public class DownloadClientConfigModule : RadarrConfigModule<DownloadClientConfigResource>
    {
        public DownloadClientConfigModule(IConfigService configService)
            : base(configService)
        {

        }

        protected override DownloadClientConfigResource ToResource(IConfigService model)
        {
            return DownloadClientConfigResourceMapper.ToResource(model);
        }
    }
}
