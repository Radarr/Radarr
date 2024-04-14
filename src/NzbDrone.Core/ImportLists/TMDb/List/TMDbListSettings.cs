using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TMDb.List
{
    public class TMDbListSettingsValidator : TMDbSettingsBaseValidator<TMDbListSettings>
    {
        public TMDbListSettingsValidator()
        {
            RuleFor(c => c.ListId).Matches("^[1-9][0-9]*$").NotEmpty();
        }
    }

    public class TMDbListSettings : TMDbSettingsBase<TMDbListSettings>
    {
        private static readonly TMDbListSettingsValidator Validator = new ();

        public TMDbListSettings()
        {
            ListId = "";
        }

        [FieldDefinition(1, Label = "ListId", Type = FieldType.Textbox, HelpText = "TMDb Id of List to Follow")]
        public string ListId { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
