using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Filmweb.User
{
    public class FilmwebUserSettingsValidator : FilmwebSettingsBaseValidator<FilmwebUserSettings>
    {
        public FilmwebUserSettingsValidator()
        {
            RuleFor(c => c.FilmwebListType).NotNull();
        }
    }

    public class FilmwebUserSettings : FilmwebSettingsBase<FilmwebUserSettings>
    {
        private static readonly FilmwebUserSettingsValidator Validator = new ();

        public FilmwebUserSettings()
        {
            FilmwebListType = (int)FilmwebUserListType.WantToSee;
        }

        [FieldDefinition(2, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(FilmwebUserListType), HelpText = "Type of list to import from Filmweb")]
        public int FilmwebListType { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
