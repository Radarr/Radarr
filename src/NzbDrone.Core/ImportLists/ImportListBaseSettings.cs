using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListBaseSettingsValidator : AbstractValidator<ImportListBaseSettings>
    {
        public ImportListBaseSettingsValidator()
        {
            RuleFor(c => c.Link).NotEmpty();
        }
    }

    public class ImportListBaseSettings : IProviderConfig
    {
        private static readonly ImportListBaseSettingsValidator Validator = new ImportListBaseSettingsValidator();

        public ImportListBaseSettings()
        {
            Link = "http://rss.imdb.com/list/";
        }

        [FieldDefinition(0, Label = "Link", HelpText = "Link to the list of movies.")]
        public string Link { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Link);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
