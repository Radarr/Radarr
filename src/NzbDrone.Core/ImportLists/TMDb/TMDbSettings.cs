using FluentValidation;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TMDb
{
    public class TMDbSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
        where TSettings : TMDbSettingsBase<TSettings>
    {
    }

    public class TMDbSettingsBase<TSettings> : ImportListSettingsBase<TSettings>
        where TSettings : TMDbSettingsBase<TSettings>
    {
        private static readonly TMDbSettingsBaseValidator<TSettings> Validator = new ();

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
