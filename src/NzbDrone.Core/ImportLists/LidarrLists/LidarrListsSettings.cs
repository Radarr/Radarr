using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.LidarrLists
{
    public class LidarrListsSettingsValidator : AbstractValidator<LidarrListsSettings>
    {
        public LidarrListsSettingsValidator()
        {
        }
    }

    public class LidarrListsSettings : IImportListSettings
    {
        private static readonly LidarrListsSettingsValidator Validator = new LidarrListsSettingsValidator();

        public LidarrListsSettings()
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
