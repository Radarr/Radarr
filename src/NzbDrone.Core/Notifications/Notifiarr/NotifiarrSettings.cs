using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
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

    public class NotifiarrSettings : IProviderConfig
    {
        private static readonly NotifiarrSettingsValidator Validator = new NotifiarrSettingsValidator();

        [FieldDefinition(0, Label = "API Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Your API key from your profile", HelpLink = "https://notifiarr.com")]
        public string APIKey { get; set; }
        [FieldDefinition(1, Label = "Instance Name", Advanced = true, HelpText = "Unique name for this instance", HelpLink = "https://notifiarr.com")]
        public string InstanceName { get; set; }
        [FieldDefinition(2, Label = "Environment", Advanced = true, Type = FieldType.Select, SelectOptions = typeof(NotifiarrEnvironment), HelpText = "Live unless told otherwise", HelpLink = "https://notifiarr.com")]
        public int Environment { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
