using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.ReadarrLists
{
    public class ReadarrListsSettingsValidator : AbstractValidator<ReadarrListsSettings>
    {
        public ReadarrListsSettingsValidator()
        {
        }
    }

    public class ReadarrListsSettings : IImportListSettings
    {
        private static readonly ReadarrListsSettingsValidator Validator = new ReadarrListsSettingsValidator();

        public ReadarrListsSettings()
        {
            BaseUrl = "";
        }

        public string BaseUrl { get; set; }

        [FieldDefinition(0, Label = "List Id", Advanced = true)]
        public string ListId { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
