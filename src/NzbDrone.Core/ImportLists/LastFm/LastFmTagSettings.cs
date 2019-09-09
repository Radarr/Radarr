using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.LastFm
{
    public class LastFmTagSettingsValidator : AbstractValidator<LastFmTagSettings>
    {
        public LastFmTagSettingsValidator()
        {
            RuleFor(c => c.TagId).NotEmpty();
            RuleFor(c => c.Count).LessThanOrEqualTo(1000);
        }
    }

    public class LastFmTagSettings : IImportListSettings
    {
        private static readonly LastFmTagSettingsValidator Validator = new LastFmTagSettingsValidator();

        public LastFmTagSettings()
        {
            BaseUrl = "http://ws.audioscrobbler.com/2.0/?method=tag.gettopartists";
            ApiKey = "204c76646d6020eee36bbc51a2fcd810";
            Count = 25;
        }

        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }

        [FieldDefinition(0, Label = "Last.fm Tag", HelpText = "Tag to pull artists from")]
        public string TagId { get; set; }

        [FieldDefinition(1, Label = "Count", HelpText = "Number of results to pull from list (Max 1000)", Type = FieldType.Number)]
        public int Count { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
