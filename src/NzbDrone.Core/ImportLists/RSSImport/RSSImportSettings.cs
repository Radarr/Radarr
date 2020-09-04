using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.RSSImport
{
    public class RSSImportSettingsValidator : AbstractValidator<RSSImportSettings>
    {
        public RSSImportSettingsValidator()
        {
            RuleFor(c => c.Link).ValidRootUrl();
        }
    }

    public class RSSImportSettings : IProviderConfig
    {
        private static readonly RSSImportSettingsValidator Validator = new RSSImportSettingsValidator();

        public RSSImportSettings()
        {
            Link = "https://rss.yoursite.com";
        }

        [FieldDefinition(0, Label = "RSS Link", HelpText = "Link to the rss feed of movies.")]
        public string Link { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
