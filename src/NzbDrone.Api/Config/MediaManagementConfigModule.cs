using FluentValidation;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.Config
{
    public class MediaManagementConfigModule : NzbDroneConfigModule<MediaManagementConfigResource>
    {
        public MediaManagementConfigModule(IConfigService configService,
                                           PathExistsValidator pathExistsValidator,
                                           FolderChmodValidator folderChmodValidator,
                                           FolderWritableValidator folderWritableValidator,
                                           MoviePathValidator moviePathValidator,
                                           StartupFolderValidator startupFolderValidator,
                                           SystemFolderValidator systemFolderValidator,
                                           RootFolderAncestorValidator rootFolderAncestorValidator,
                                           RootFolderValidator rootFolderValidator)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.ChmodFolder).SetValidator(folderChmodValidator).When(c => !string.IsNullOrEmpty(c.ChmodFolder) && PlatformInfo.IsMono);
            SharedValidator.RuleFor(c => c.RecycleBin).IsValidPath()
                                                      .SetValidator(folderWritableValidator)
                                                      .SetValidator(rootFolderValidator)
                                                      .SetValidator(pathExistsValidator)
                                                      .SetValidator(moviePathValidator)
                                                      .SetValidator(rootFolderAncestorValidator)
                                                      .SetValidator(startupFolderValidator)
                                                      .SetValidator(systemFolderValidator)
                                                      .When(c => !string.IsNullOrWhiteSpace(c.RecycleBin));
        }

        protected override MediaManagementConfigResource ToResource(IConfigService model)
        {
            return MediaManagementConfigResourceMapper.ToResource(model);
        }
    }
}
