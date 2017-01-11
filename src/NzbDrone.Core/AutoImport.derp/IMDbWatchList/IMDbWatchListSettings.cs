using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.AutoImport.IMDbWatchList
{
    public class IMDbWatchListSettingsValidator : AbstractValidator<IMDbWatchListSettings>
    {
        public IMDbWatchListSettingsValidator()
        {
            RuleFor(c => c.IMDbWatchListId).NotEmpty();
        }
    }

    public class IMDbWatchListSettings : IProviderConfig
    {
        private static readonly IMDbWatchListSettingsValidator Validator = new IMDbWatchListSettingsValidator();

        [FieldDefinition(0, Label = "Watch List Id", HelpLink = "http://rss.imdb.com/list/")]
        public string IMDbWatchListId { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(IMDbWatchListId);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
