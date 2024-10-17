using System;
using System.Collections.Generic;
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

            RuleFor(c => c.MinVoteAverage)
                .InclusiveBetween(0, 10)
                .When(c => c.MinVoteAverage.HasValue)
                .WithMessage("Minimum vote average must be between 0.0 and 10.0");

            RuleFor(c => c.MinVotes)
                .GreaterThan(0)
                .When(c => c.MinVotes.HasValue)
                .WithMessage("Minimum votes must be greater than 0");
        }
    }

    public class TMDbPersonSettings : TMDbSettingsBase<TMDbPersonSettings>
    {
        private static readonly TMDbPersonSettingsValidator Validator = new ();

        public TMDbPersonSettings()
        {
            PersonId = "";
        }

        [FieldDefinition(1, Label = "Person Id", Type = FieldType.Textbox, HelpText = "TMDb Id of Person to Follow")]
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

        [FieldDefinition(7, Label = "Minimum Vote Average", HelpText = "Filter movies by votes (0.0-10.0)")]
        public double? MinVoteAverage { get; set; }

        [FieldDefinition(8, Label = "Minimum Number of Votes", HelpText = "Filter movies by number of votes")]
        public int? MinVotes { get; set; }

        [FieldDefinition(9, Label = "Genres", Type = FieldType.Select, SelectOptionsProviderAction = "getTmdbGenres", HelpText = "Filter movies by TMDb Genre Ids")]
        public IEnumerable<int> GenreIds { get; set; } = Array.Empty<int>();

        [FieldDefinition(10, Label = "Original Languages", Type = FieldType.Select, SelectOptionsProviderAction = "getTmdbLanguages", HelpText = "Filter by Languages")]
        public IEnumerable<string> LanguageCodes { get; set; } = Array.Empty<string>();

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
