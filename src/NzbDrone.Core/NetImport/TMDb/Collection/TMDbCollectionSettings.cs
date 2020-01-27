using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.NetImport.TMDb.Collection
{
    public class TMDbCollectionSettingsValidator : TMDbSettingsBaseValidator<TMDbCollectionSettings>
    {
        public TMDbCollectionSettingsValidator()
        : base()
        {
            RuleFor(c => c.CollectionId).Matches(@"^[1-9][0-9]*$", RegexOptions.IgnoreCase);
        }
    }

    public class TMDbCollectionSettings : TMDbSettingsBase<TMDbCollectionSettings>
    {
        protected override AbstractValidator<TMDbCollectionSettings> Validator => new TMDbCollectionSettingsValidator();

        public TMDbCollectionSettings()
        {
            CollectionId = "";
        }

        [FieldDefinition(1, Label = "Collection Id", Type = FieldType.Textbox, HelpText = "TMDb Id of Collection to Follow")]
        public string CollectionId { get; set; }
    }
}
