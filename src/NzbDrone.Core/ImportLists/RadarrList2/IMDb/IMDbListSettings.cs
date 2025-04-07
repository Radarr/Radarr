using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.RadarrList2.IMDbList
{
    public class IMDbSettingsValidator : AbstractValidator<IMDbListSettings>
    {
        public IMDbSettingsValidator()
        {
            RuleFor(c => c.ListId).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Matches(@"^(top250|popular)$|^ur\d+$")
                .WithMessage("List ID must be 'top250', 'popular' or an IMDb user watchlist of the form 'ur12345678'");
        }
    }

    public class IMDbListSettings : ImportListSettingsBase<IMDbListSettings>
    {
        private static readonly IMDbSettingsValidator Validator = new ();

        [FieldDefinition(1, Label = "List/User ID", HelpText = "IMDb user ID (e.g. ur12345678), 'top250' or 'popular'")]
        public string ListId { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
