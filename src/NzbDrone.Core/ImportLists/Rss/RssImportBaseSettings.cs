using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Rss
{
    public class RssImportSettingsValidator : AbstractValidator<RssImportBaseSettings>
    {
        public RssImportSettingsValidator()
        {
            RuleFor(c => c.Url).NotEmpty();
        }
    }

    public class RssImportBaseSettings : IProviderConfig
    {
        private RssImportSettingsValidator Validator => new ();

        [FieldDefinition(0, Label = "Url", Type = FieldType.Textbox)]
        public virtual string Url { get; set; }

        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
