using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TMDb.Company
{
    public class TMDbCompanySettingsValidator : TMDbSettingsBaseValidator<TMDbCompanySettings>
    {
        public TMDbCompanySettingsValidator()
        {
            RuleFor(c => c.CompanyId).Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase);
        }
    }

    public class TMDbCompanySettings : TMDbSettingsBase<TMDbCompanySettings>
    {
        private static readonly TMDbCompanySettingsValidator Validator = new ();

        public TMDbCompanySettings()
        {
            CompanyId = "";
        }

        [FieldDefinition(1, Label = "Company Id", Type = FieldType.Textbox, HelpText = "TMDb Id of Company to Follow")]
        public string CompanyId { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
