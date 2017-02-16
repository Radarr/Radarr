﻿using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.TMDb
{

    public class TMDbSettingsValidator : AbstractValidator<TMDbSettings>
    {
        public TMDbSettingsValidator()
        {
            RuleFor(c => double.Parse(c.MinVoteAverage)).InclusiveBetween(0, 10);
            RuleFor(c => c.MinVotes).GreaterThan(0);
        }
    }

    public class TMDbSettings : NetImportBaseSettings
    {
        private static readonly TMDbSettingsValidator Validator = new TMDbSettingsValidator();

        public TMDbSettings()
        {
            MinVoteAverage = "5.5";
            MinVotes = 1000;
            LanguageCode = (int)TMDbLanguageCodes.en;
        }

        [FieldDefinition(0, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TMDbListType), HelpText = "Type of list your seeking to import from")]
        public int ListType { get; set; }

        [FieldDefinition(1, Label = "Public List ID", HelpText = "Required for List (Ignores Filtering Options)")]
        public string ListId { get; set; }

        [FieldDefinition(2, Label = "Minimum Vote Average", HelpText = "Filter movies by votes (0.0-10.0)")]
        public string MinVoteAverage { get; set; }

        [FieldDefinition(3, Label = "Minimum Number of Votes", HelpText = "Filter movies by number of votes")]
        public int MinVotes { get; set; }

        [FieldDefinition(4, Label = "Rating", HelpText = "Filter movies by a rating (NR,G,PG,PG-13,R,NC-17)")]
        public string Ceritification { get; set; }

        [FieldDefinition(5, Label = "Include Genre Ids", HelpText = "Filter movies by TMDb Genre Ids (Comma Separated)")]
        public string IncludeGenreIds { get; set; }

        [FieldDefinition(6, Label = "Exclude Genre Ids", HelpText = "Filter movies by TMDb Genre Ids (Comma Separated)")]
        public string ExcludeGenreIds { get; set; }

        [FieldDefinition(7, Label = "Language Code", Type = FieldType.Select, SelectOptions = typeof(TMDbLanguageCodes), HelpText = "Filter by Language")]
        public int LanguageCode { get; set; }

        public new NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

}
