using FluentValidation;
using Radarr.Http.Validation;
using NzbDrone.Core.Configuration;

namespace Radarr.Api.V2.Config
{
    public class TraktAuthenticationConfigModule : RadarrConfigModule<TraktAuthenticationConfigResource>
    {

        public TraktAuthenticationConfigModule(IConfigService configService)
            : base(configService)
        {
        }

        protected override TraktAuthenticationConfigResource ToResource(IConfigService model)
        {
            return TraktAuthenticationConfigResourceMapper.ToResource(model);
        }
    }
}
