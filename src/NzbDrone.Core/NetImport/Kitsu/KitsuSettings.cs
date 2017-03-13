using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport.Kitsu
{
    public class KitsuSettingsValidator : AbstractValidator<KitsuSettings>
    {
        public KitsuSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();
        }
    }

    public class KitsuSettings : IProviderConfig
    {
        private static readonly KitsuSettingsValidator Validator = new KitsuSettingsValidator();

        public KitsuSettings()
        {
            Username = "";
        }

        [FieldDefinition(0, Label = "List", Type = FieldType.Select, SelectOptions = typeof(KitsuListType), HelpText = "Kitsu List")]
        public int List { get; set; }

        [FieldDefinition(1, Label = "Username", HelpText = "Kitsu Username")]
        public string Username { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Username);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
