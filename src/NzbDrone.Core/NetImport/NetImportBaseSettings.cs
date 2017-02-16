using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport
{
    public class NetImportBaseSettingsValidator : AbstractValidator<NetImportBaseSettings>
    {
        public NetImportBaseSettingsValidator()
        { }
    }

    public class NetImportBaseSettings : IProviderConfig
    {
        private static readonly NetImportBaseSettingsValidator Validator = new NetImportBaseSettingsValidator();

        public NetImportBaseSettings()
        { }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
