using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.RadarrList
{
    public class RadarrSettingsValidator : AbstractValidator<RadarrListSettings>
    {
        public RadarrSettingsValidator()
        {
            RuleFor(c => c.Url).ValidRootUrl();
        }
    }

    public class RadarrListSettings : ImportListSettingsBase<RadarrListSettings>
    {
        private static readonly RadarrSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "List URL", HelpText = "The URL for the movie list")]
        public string Url { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
