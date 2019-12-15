using FluentValidation;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
        where TSettings : TMDbSettingsBase<TSettings>
    {
        public TMDbSettingsBaseValidator()
        {
        }
    }

    public class TMDbSettingsBase<TSettings> : IProviderConfig
        where TSettings : TMDbSettingsBase<TSettings>
    {
        protected virtual AbstractValidator<TSettings> Validator => new TMDbSettingsBaseValidator<TSettings>();
        public TMDbSettingsBase()
        {
        }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
