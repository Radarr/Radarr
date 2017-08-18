using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Boxcar
{
    public class BoxcarSettingsValidator : AbstractValidator<BoxcarSettings>
    {
        public BoxcarSettingsValidator()
        {
            RuleFor(c => c.Token).NotEmpty();
        }
    }

    public class BoxcarSettings : IProviderConfig
    {
        private static readonly BoxcarSettingsValidator Validator = new BoxcarSettingsValidator();

        [FieldDefinition(0, Label = "Access Token", HelpText = "Your access token, from your Boxcar account settings.", HelpLink = "https://new.boxcar.io/account/edit")]
        public string Token { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
