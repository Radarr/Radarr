using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.ImportLists.TMDb.Company
{
    public class TMDbCompanySettingsValidator : TMDbSettingsBaseValidator<TMDbCompanySettings>
    {
        public TMDbCompanySettingsValidator()
        : base()
        {
            RuleFor(c => c.CompanyId).Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase);
        }
    }

    public class TMDbCompanySettings : TMDbSettingsBase<TMDbCompanySettings>
    {
        protected override AbstractValidator<TMDbCompanySettings> Validator => new TMDbCompanySettingsValidator();

        public TMDbCompanySettings()
        {
            CompanyId = "";
        }

        [FieldDefinition(1, Label = "Company Id", Type = FieldType.Textbox, HelpText = "TMDb Id of Company to Follow")]
        public string CompanyId { get; set; }
    }
}
