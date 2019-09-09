using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.HeadphonesImport
{
    public class HeadphonesImportSettingsValidator : AbstractValidator<HeadphonesImportSettings>
    {
        public HeadphonesImportSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class HeadphonesImportSettings : IImportListSettings
    {
        private static readonly HeadphonesImportSettingsValidator Validator = new HeadphonesImportSettingsValidator();

        public HeadphonesImportSettings()
        {
            BaseUrl = "http://localhost:8181/";
        }

        [FieldDefinition(0, Label = "Headphones URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string ApiKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
