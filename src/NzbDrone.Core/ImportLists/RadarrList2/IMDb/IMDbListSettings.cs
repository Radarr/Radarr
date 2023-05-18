using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.RadarrList2.IMDbList
{
    public class IMDbSettingsValidator : AbstractValidator<IMDbListSettings>
    {
        public IMDbSettingsValidator()
        {
            RuleFor(c => c.ListId)
                .Matches(@"^top250$|^popular$|^ls\d+$|^ur\d+$")
                .WithMessage("List ID mist be 'top250', 'popular', an IMDb List ID of the form 'ls12345678' or an IMDb user watchlist of the form 'ur12345678'");
        }
    }

    public class IMDbListSettings : IProviderConfig
    {
        private static readonly IMDbSettingsValidator Validator = new IMDbSettingsValidator();

        [FieldDefinition(1, Label = "List/User ID", HelpText = "IMDb list ID (e.g ls12345678), IMDb user ID (e.g. ur12345678), 'top250' or 'popular'")]
        public string ListId { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
