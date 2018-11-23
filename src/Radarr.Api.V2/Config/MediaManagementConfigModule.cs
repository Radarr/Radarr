using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation.Paths;

namespace Radarr.Api.V2.Config
{
    public class MediaManagementConfigModule : RadarrConfigModule<MediaManagementConfigResource>
    {
        public MediaManagementConfigModule(IConfigService configService, PathExistsValidator pathExistsValidator)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.FileChmod).NotEmpty();
            SharedValidator.RuleFor(c => c.FolderChmod).NotEmpty();
            SharedValidator.RuleFor(c => c.RecycleBin).IsValidPath().SetValidator(pathExistsValidator).When(c => !string.IsNullOrWhiteSpace(c.RecycleBin));
        }

        protected override MediaManagementConfigResource ToResource(IConfigService model)
        {
            return MediaManagementConfigResourceMapper.ToResource(model);
        }
    }
}
