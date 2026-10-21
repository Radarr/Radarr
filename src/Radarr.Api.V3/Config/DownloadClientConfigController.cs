using FluentValidation;
using NzbDrone.Core.Configuration;
using Radarr.Http;

namespace Radarr.Api.V3.Config
{
    [V3ApiController("config/downloadclient")]
    public class DownloadClientConfigController : ConfigController<DownloadClientConfigResource>
    {
        public DownloadClientConfigController(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.StalledTorrentTimeout).GreaterThanOrEqualTo(0);
            SharedValidator.RuleFor(c => c.StalledTorrentTimeout)
                           .GreaterThanOrEqualTo(10)
                           .When(c => c.StalledTorrentTimeout != 0)
                           .WithMessage("Must be 0 (disabled) or at least 10 minutes");
        }

        protected override DownloadClientConfigResource ToResource(IConfigService model)
        {
            return DownloadClientConfigResourceMapper.ToResource(model);
        }
    }
}
