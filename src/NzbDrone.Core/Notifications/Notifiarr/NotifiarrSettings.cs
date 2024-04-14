using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public class NotifiarrSettingsValidator : AbstractValidator<NotifiarrSettings>
    {
        public NotifiarrSettingsValidator()
        {
            RuleFor(c => c.APIKey).NotEmpty();
        }
    }

    public class NotifiarrSettings : NotificationSettingsBase<NotifiarrSettings>
    {
        private static readonly NotifiarrSettingsValidator Validator = new ();

        [FieldDefinition(0, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey, HelpText = "NotificationsNotifiarrSettingsApiKeyHelpText", HelpLink = "https://notifiarr.com")]
        public string APIKey { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
