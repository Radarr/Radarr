﻿using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.NetImport.TMDb.Person
{
    public class TMDbPersonSettingsValidator : TMDbSettingsBaseValidator<TMDbPersonSettings>
    {
        public TMDbPersonSettingsValidator()
        : base()
        {
            RuleFor(c => c.PersonId).Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase);
        }
    }

    public class TMDbPersonSettings : TMDbSettingsBase<TMDbPersonSettings>
    {
        protected override AbstractValidator<TMDbPersonSettings> Validator => new TMDbPersonSettingsValidator();

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
    }
}
