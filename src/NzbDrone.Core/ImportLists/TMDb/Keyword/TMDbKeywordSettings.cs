using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.TMDb.Keyword
{
    public class TMDbKeywordSettingsValidator : TMDbSettingsBaseValidator<TMDbKeywordSettings>
    {
        public TMDbKeywordSettingsValidator()
        : base()
        {
            RuleFor(c => c.KeywordId).Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase);
        }
    }

    public class TMDbKeywordSettings : TMDbSettingsBase<TMDbKeywordSettings>
    {
        protected override AbstractValidator<TMDbKeywordSettings> Validator => new TMDbKeywordSettingsValidator();

        public TMDbKeywordSettings()
        {
            KeywordId = "";
        }

        [FieldDefinition(1, Label = "Keyword Id", Type = FieldType.Textbox, HelpText = "TMDb Id of keyword to Follow")]
        public string KeywordId { get; set; }
    }
}
