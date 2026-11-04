using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Filmweb
{
    public class FilmwebSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
        where TSettings : FilmwebSettingsBase<TSettings>
    {
        public FilmwebSettingsBaseValidator()
        {
            RuleFor(c => c.Username).NotEmpty()
                                   .WithMessage("Username is required");

            RuleFor(c => c.Limit)
                .GreaterThan(0)
                .LessThanOrEqualTo(1000)
                .WithMessage("Must be integer between 1 and 1000");
        }
    }

    public class FilmwebSettingsBase<TSettings> : ImportListSettingsBase<TSettings>
        where TSettings : FilmwebSettingsBase<TSettings>
    {
        private static readonly FilmwebSettingsBaseValidator<TSettings> Validator = new ();

        public FilmwebSettingsBase()
        {
            Limit = 100;
        }

        public string Link => "https://www.filmweb.pl";

        [FieldDefinition(1, Label = "Username", HelpText = "Filmweb username")]
        public string Username { get; set; }

        [FieldDefinition(98, Label = "Limit", HelpText = "Limit the number of movies to get")]
        public int Limit { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
