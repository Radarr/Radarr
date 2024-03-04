using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Rss.Imdb
{
    public class ImdbRssImportSettingsValidator : AbstractValidator<ImdbRssImportSettings>
    {
        public ImdbRssImportSettingsValidator()
        {
            RuleFor(c => c.Url).NotEmpty();
        }
    }

    public class ImdbRssImportSettings : RssImportBaseSettings
    {
        private ImdbRssImportSettingsValidator Validator => new ();

        [FieldDefinition(0, Label = "Url", Type = FieldType.Textbox)]
        public override string Url { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
