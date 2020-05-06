using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.LazyLibrarianImport
{
    public class LazyLibrarianImportSettingsValidator : AbstractValidator<LazyLibrarianImportSettings>
    {
        public LazyLibrarianImportSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).IsValidUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class LazyLibrarianImportSettings : IImportListSettings
    {
        private static readonly LazyLibrarianImportSettingsValidator Validator = new LazyLibrarianImportSettingsValidator();

        public LazyLibrarianImportSettings()
        {
            BaseUrl = "http://localhost:5299";
        }

        [FieldDefinition(0, Label = "Url")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string ApiKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
