using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport
{
    public class NetImportBaseSettingsValidator : AbstractValidator<NetImportBaseSettings>
    {
        public NetImportBaseSettingsValidator()
        {
            RuleFor(c => c.Link).NotEmpty();
        }
    }

    public class NetImportBaseSettings : IProviderConfig
    {
        private static readonly NetImportBaseSettingsValidator Validator = new NetImportBaseSettingsValidator();

        public NetImportBaseSettings()
        {
            Link = "http://rss.imdb.com/list/";
            ProfileId = 1;
        }

        [FieldDefinition(0, Label = "Link", HelpText = "Link to the list of movies.")]
        public string Link { get; set; }

        [FieldDefinition(1, Label = "Quality", Type = FieldType.Select, SelectOptions = typeof(Profile), HelpText = "Quality of all imported movies")]
        public int ProfileId { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Link);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
