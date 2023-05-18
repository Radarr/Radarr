using FluentValidation;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TMDb
{
    public class TMDbSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
        where TSettings : TMDbSettingsBase<TSettings>
    {
    }

    public class TMDbSettingsBase<TSettings> : IProviderConfig
        where TSettings : TMDbSettingsBase<TSettings>
    {
        protected virtual AbstractValidator<TSettings> Validator => new TMDbSettingsBaseValidator<TSettings>();

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
