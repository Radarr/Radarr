using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.LastFm
{
    public class LastFmSettingsValidator : AbstractValidator<LastFmUserSettings>
    {
        public LastFmSettingsValidator()
        {
            RuleFor(c => c.UserId).NotEmpty();
            RuleFor(c => c.Count).LessThanOrEqualTo(1000);
        }
    }

    public class LastFmUserSettings : IImportListSettings
    {
        private static readonly LastFmSettingsValidator Validator = new LastFmSettingsValidator();

        public LastFmUserSettings()
        {
            BaseUrl = "http://ws.audioscrobbler.com/2.0/?method=user.gettopartists";
            ApiKey = "204c76646d6020eee36bbc51a2fcd810";
            Count = 25;
        }

        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }

        [FieldDefinition(0, Label = "Last.fm UserID", HelpText = "Last.fm UserId to pull artists from")]
        public string UserId { get; set; }

        [FieldDefinition(1, Label = "Count", HelpText = "Number of results to pull from list (Max 1000)", Type = FieldType.Number)]
        public int Count { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
