using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.RadarrList
{
    public class RadarrSettingsValidator : AbstractValidator<RadarrListSettings>
    {
        public RadarrSettingsValidator()
        {
            RuleFor(c => c.Url).ValidRootUrl();
        }
    }

    public class RadarrListSettings : IProviderConfig
    {
        private static readonly RadarrSettingsValidator Validator = new RadarrSettingsValidator();

        [FieldDefinition(0, Label = "List URL", HelpText = "The URL for the movie list")]
        public string Url { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
