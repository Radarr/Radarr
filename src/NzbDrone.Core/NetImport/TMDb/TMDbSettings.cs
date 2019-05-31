using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.NetImport.TMDb
{

    public class TMDbSettingsValidator : AbstractValidator<TMDbSettings>
    {
        public TMDbSettingsValidator()
        {
            RuleFor(c => c.Link).ValidRootUrl();

            // Greater than 0
            RuleFor(c => c.ListId)
                .Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase)
                .When(c => c.ListType == (int)TMDbListType.List)
                .WithMessage("List Id is required when using TMDb Lists");

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

    public class TMDbSettings : IProviderConfig
    {
        private static readonly TMDbSettingsValidator Validator = new TMDbSettingsValidator();

        public TMDbSettings()
        {
            Link = "https://api.themoviedb.org";
            ListType = (int)TMDbListType.Popular;
            MinVoteAverage = "5";
            MinVotes = "1";
            LanguageCode = (int)TMDbLanguageCodes.en;
            AccountID = "https://github.com/elChapoSing/tmdb_account_id"
        }

        [FieldDefinition(0, Label = "TMDb API URL", HelpText = "Link to to TMDb API URL, do not change unless you know what you are doing.")]
        public string Link { get; set; }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TMDbListType), HelpText = "Type of list your seeking to import from")]
        public int ListType { get; set; }

        [FieldDefinition(2, Label = "Public List ID", HelpText = "Required for List (Ignores Filtering Options)")]
        public string ListId { get; set; }

        [FieldDefinition(3, Label = "Minimum Vote Average", HelpText = "Filter movies by votes (0.0-10.0)")]
        public string MinVoteAverage { get; set; }

        [FieldDefinition(4, Label = "Minimum Number of Votes", HelpText = "Filter movies by number of votes")]
        public string MinVotes { get; set; }

        [FieldDefinition(5, Label = "Certification", HelpText = "Filter movies by a single ceritification (NR,G,PG,PG-13,R,NC-17)")]
        public string Ceritification { get; set; }

        [FieldDefinition(6, Label = "Include Genre Ids", HelpText = "Filter movies by TMDb Genre Ids (Comma Separated)")]
        public string IncludeGenreIds { get; set; }

        [FieldDefinition(7, Label = "Exclude Genre Ids", HelpText = "Filter movies by TMDb Genre Ids (Comma Separated)")]
        public string ExcludeGenreIds { get; set; }

        [FieldDefinition(8, Label = "Original Language", Type = FieldType.Select, SelectOptions = typeof(TMDbLanguageCodes), HelpText = "Filter by Language")]
        public int LanguageCode { get; set; }

        [FieldDefinition(9, Label = "Account ID", HelpText = "Your account ID for Radarr API. Use the link to get it.")]
        public string AccountID { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

}
