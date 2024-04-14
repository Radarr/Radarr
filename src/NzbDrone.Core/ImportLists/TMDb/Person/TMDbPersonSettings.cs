using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TMDb.Person
{
    public class TMDbPersonSettingsValidator : TMDbSettingsBaseValidator<TMDbPersonSettings>
    {
        public TMDbPersonSettingsValidator()
        {
            RuleFor(c => c.PersonId).Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase);

            RuleFor(c => c.PersonCast)
                .Equal(true)
                .Unless(c => c.PersonCastDirector || c.PersonCastProducer || c.PersonCastSound || c.PersonCastWriting)
                .WithMessage("Must Select One Credit Type Option");
            RuleFor(c => c.PersonCastDirector)
                .Equal(true)
                .Unless(c => c.PersonCast || c.PersonCastProducer || c.PersonCastSound || c.PersonCastWriting)
                .WithMessage("Must Select One Credit Type Option");
            RuleFor(c => c.PersonCastProducer)
                .Equal(true)
                .Unless(c => c.PersonCastDirector || c.PersonCast || c.PersonCastSound || c.PersonCastWriting)
                .WithMessage("Must Select One Credit Type Option");
            RuleFor(c => c.PersonCastSound)
                .Equal(true)
                .Unless(c => c.PersonCastDirector || c.PersonCastProducer || c.PersonCast || c.PersonCastWriting)
                .WithMessage("Must Select One Credit Type Option");
            RuleFor(c => c.PersonCastWriting)
                .Equal(true)
                .Unless(c => c.PersonCastDirector || c.PersonCastProducer || c.PersonCastSound || c.PersonCast)
                .WithMessage("Must Select One Credit Type Option");
        }
    }

    public class TMDbPersonSettings : TMDbSettingsBase<TMDbPersonSettings>
    {
        private static readonly TMDbPersonSettingsValidator Validator = new ();

        public TMDbPersonSettings()
        {
            PersonId = "";
        }

        [FieldDefinition(1, Label = "PersonId", Type = FieldType.Textbox, HelpText = "TMDb Id of Person to Follow")]
        public string PersonId { get; set; }

        [FieldDefinition(2, Label = "Person Cast", HelpText = "Select if you want to include Cast credits", Type = FieldType.Checkbox)]
        public bool PersonCast { get; set; }

        [FieldDefinition(3, Label = "Person Director Credits", HelpText = "Select if you want to include Director credits", Type = FieldType.Checkbox)]
        public bool PersonCastDirector { get; set; }

        [FieldDefinition(4, Label = "Person Producer Credits", HelpText = "Select if you want to include Producer credits", Type = FieldType.Checkbox)]
        public bool PersonCastProducer { get; set; }

        [FieldDefinition(5, Label = "Person Sound Credits", HelpText = "Select if you want to include Sound credits", Type = FieldType.Checkbox)]
        public bool PersonCastSound { get; set; }

        [FieldDefinition(6, Label = "Person Writing Credits", HelpText = "Select if you want to include Writing credits", Type = FieldType.Checkbox)]
        public bool PersonCastWriting { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
