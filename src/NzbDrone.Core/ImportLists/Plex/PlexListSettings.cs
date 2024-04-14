using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Plex
{
    public class PlexListSettingsValidator : AbstractValidator<PlexListSettings>
    {
        public PlexListSettingsValidator()
        {
            RuleFor(c => c.AccessToken).NotEmpty()
                           .OverridePropertyName("SignIn")
                           .WithMessage("Must authenticate with Plex");
        }
    }

    public class PlexListSettings : ImportListSettingsBase<PlexListSettings>
    {
        private static readonly PlexListSettingsValidator Validator = new ();

        public PlexListSettings()
        {
            SignIn = "startOAuth";
        }

        public virtual string Scope => "";

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(99, Label = "Authenticate with Plex.tv", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
