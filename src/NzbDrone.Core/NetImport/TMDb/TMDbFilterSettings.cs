using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.NetImport.TMDb
{
    public class TMDbFilterSettingsValidator : AbstractValidator<TMDbFilterSettings>
    {
        public TMDbFilterSettingsValidator()
        {
            // Range 0.0 - 10.0
            RuleFor(c => c.MinVoteAverage)
                .Matches(@"^(?!0\d)\d*(\.\d{1})?$", RegexOptions.IgnoreCase)
                .When(c => c.MinVoteAverage.IsNotNullOrWhiteSpace())
                .WithMessage("Minimum vote average must be between 0 and 10");

            // Greater than 0
            RuleFor(c => c.MinVotes)
                .Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase)
                .When(c => c.MinVotes.IsNotNullOrWhiteSpace())
                .WithMessage("Minimum votes must be greater than 0");

            // Any valid certification
            RuleFor(c => c.Ceritification)
                .Matches(@"^\bNR\b|\bG\b|\bPG\b|\bPG\-13\b|\bR\b|\bNC\-17\b$", RegexOptions.IgnoreCase)
                .When(c => c.Ceritification.IsNotNullOrWhiteSpace())
                .WithMessage("Not a valid certification");

            // CSV of numbers
            RuleFor(c => c.IncludeGenreIds)
                .Matches(@"^\d+([,|]\d+)*$", RegexOptions.IgnoreCase)
                .When(c => c.IncludeGenreIds.IsNotNullOrWhiteSpace())
                .WithMessage("Genre Ids must be comma (,) or pipe (|) separated number ids");

            // CSV of numbers
            RuleFor(c => c.ExcludeGenreIds)
                .Matches(@"^\d+([,|]\d+)*$", RegexOptions.IgnoreCase)
                .When(c => c.ExcludeGenreIds.IsNotNullOrWhiteSpace())
                .WithMessage("Genre Ids must be comma (,) or pipe (|) separated number ids");
        }
    }

    public class TMDbFilterSettings
    {
        public TMDbFilterSettings()
        {
            MinVoteAverage = "5";
            MinVotes = "1";
            LanguageCode = (int)TMDbLanguageCodes.en;
            ExcludeGenreIds = "";
            IncludeGenreIds = "";
        }

        [FieldDefinition(1, Label = "Minimum Vote Average", HelpText = "Filter movies by votes (0.0-10.0)")]
        public string MinVoteAverage { get; set; }

        [FieldDefinition(2, Label = "Minimum Number of Votes", HelpText = "Filter movies by number of votes")]
        public string MinVotes { get; set; }

        [FieldDefinition(3, Label = "Certification", HelpText = "Filter movies by a single ceritification (NR,G,PG,PG-13,R,NC-17)")]
        public string Ceritification { get; set; }

        [FieldDefinition(4, Label = "Include Genre Ids", HelpText = "Filter movies by TMDb Genre Ids (Comma Separated)")]
        public string IncludeGenreIds { get; set; }

        [FieldDefinition(5, Label = "Exclude Genre Ids", HelpText = "Filter movies by TMDb Genre Ids (Comma Separated)")]
        public string ExcludeGenreIds { get; set; }

        [FieldDefinition(6, Label = "Original Language", Type = FieldType.Select, SelectOptions = typeof(TMDbLanguageCodes), HelpText = "Filter by Language")]
        public int LanguageCode { get; set; }
    }
}
