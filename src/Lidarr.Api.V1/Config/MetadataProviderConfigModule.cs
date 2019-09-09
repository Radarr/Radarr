using System;
using System.Linq;
using System.Reflection;
using FluentValidation;
using NzbDrone.Core.Configuration;
using Lidarr.Http;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Validation;

namespace Lidarr.Api.V1.Config
{
    public class MetadataProviderConfigModule : LidarrConfigModule<MetadataProviderConfigResource>
    {
        public MetadataProviderConfigModule(IConfigService configService)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.MetadataSource).IsValidUrl().When(c => !c.MetadataSource.IsNullOrWhiteSpace());
        }

        protected override MetadataProviderConfigResource ToResource(IConfigService model)
        {
            return MetadataProviderConfigResourceMapper.ToResource(model);
        }
    }
}
