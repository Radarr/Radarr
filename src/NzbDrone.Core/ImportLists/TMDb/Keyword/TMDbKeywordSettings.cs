using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TMDb.Keyword
{
    public class TMDbKeywordSettingsValidator : TMDbSettingsBaseValidator<TMDbKeywordSettings>
    {
        public TMDbKeywordSettingsValidator()
        {
            RuleFor(c => c.KeywordId).Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase);
        }
    }

    public class TMDbKeywordSettings : TMDbSettingsBase<TMDbKeywordSettings>
    {
        private static readonly TMDbKeywordSettingsValidator Validator = new ();

        public TMDbKeywordSettings()
        {
            KeywordId = "";
        }

        [FieldDefinition(1, Label = "Keyword Id", Type = FieldType.Textbox, HelpText = "TMDb Id of keyword to Follow")]
        public string KeywordId { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
