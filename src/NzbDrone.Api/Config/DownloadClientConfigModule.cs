using FluentValidation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Validation.Paths;

namespace NzbDrone.Api.Config
{
    public class DownloadClientConfigModule : NzbDroneConfigModule<DownloadClientConfigResource>
    {
        public DownloadClientConfigModule(IConfigService configService,
                                          RootFolderValidator rootFolderValidator,
                                          PathExistsValidator pathExistsValidator,
                                          MappedNetworkDriveValidator mappedNetworkDriveValidator)
            : base(configService)
        {
            SharedValidator.RuleFor(c => c.DownloadedAlbumsFolder)
                           .Cascade(CascadeMode.StopOnFirstFailure)
                           .IsValidPath()
                           .SetValidator(rootFolderValidator)
                           .SetValidator(mappedNetworkDriveValidator)
                           .SetValidator(pathExistsValidator)
                           .When(c => !string.IsNullOrWhiteSpace(c.DownloadedAlbumsFolder));
        }

        protected override DownloadClientConfigResource ToResource(IConfigService model)
        {
            return DownloadClientConfigResourceMapper.ToResource(model);
        }
    }
}