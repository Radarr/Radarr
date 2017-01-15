using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.IMDbWatchList
{
    public class IMDbWatchListSettingsValidator : AbstractValidator<IMDbWatchListSettings>
    {
        public IMDbWatchListSettingsValidator()
        {
            RuleFor(c => c.Link).NotEmpty();
        }
    }

    public class IMDbWatchListSettings : IProviderConfig
    {
        private static readonly IMDbWatchListSettingsValidator Validator = new IMDbWatchListSettingsValidator();

        public IMDbWatchListSettings()
        {
            Link = "http://rss.imdb.com/list/";
        }

        [FieldDefinition(0, Label = "Watch List RSS link", HelpLink = "http://rss.imdb.com/list/")]
        public string Link { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Link);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
