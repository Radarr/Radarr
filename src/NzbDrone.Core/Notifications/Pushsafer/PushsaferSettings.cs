using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Pushsafer
{
    public class PushsaferSettingsValidator : AbstractValidator<PushsaferSettings>
    {
        public PushsaferSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
            RuleFor(c => c.Retry).GreaterThanOrEqualTo(60).LessThanOrEqualTo(10800).When(c => (PushsaferPriority)c.Priority == PushsaferPriority.Emergency);
            RuleFor(c => c.Expire).GreaterThanOrEqualTo(60).LessThanOrEqualTo(10800).When(c => (PushsaferPriority)c.Priority == PushsaferPriority.Emergency);
            RuleFor(c => c.Sound).Must(c => int.TryParse(c, out var val) && val >= 0 && val <= 62).When(c => !string.IsNullOrWhiteSpace(c.Sound)).WithMessage("'Sound' must be greater than or equal to 0 and less than or equal to 62.");
            RuleFor(c => c.Vibration).Must(c => int.TryParse(c, out var val) && val >= 1 && val <= 3).When(c => !string.IsNullOrWhiteSpace(c.Vibration)).WithMessage("'Vibration must be greater than or equal to 1 and less than or equal to 3.");
            RuleFor(c => c.Icon).Must(c => int.TryParse(c, out var val) && val >= 1 && val <= 181).When(c => !string.IsNullOrWhiteSpace(c.Icon)).WithMessage("'Icon' must be be greater than or equal to 1 and less than or equal to 181.");
            RuleFor(c => c.IconColor).Matches(new Regex("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$")).When(c => !string.IsNullOrWhiteSpace(c.IconColor));
        }
    }

    public class PushsaferSettings : IProviderConfig
    {
        private static readonly PushsaferSettingsValidator Validator = new ();

        public PushsaferSettings()
        {
            Priority = (int)PushsaferPriority.Normal;
            DeviceIds = Array.Empty<string>();
        }

        [FieldDefinition(0, Label = "API Key", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://www.pushsafer.com/en/pushapi_ext#API-K")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "Device IDs", HelpText = "Device Group ID or list of Device IDs (leave blank to send to all devices)", Type = FieldType.Tag, Placeholder = "123456789|987654321")]
        public IEnumerable<string> DeviceIds { get; set; }

        [FieldDefinition(2, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(PushsaferPriority))]
        public int Priority { get; set; }

        [FieldDefinition(3, Label = "Retry", Type = FieldType.Textbox, HelpText = "Interval to retry Emergency alerts, minimum 60 seconds")]
        public int Retry { get; set; }

        [FieldDefinition(4, Label = "Expire", Type = FieldType.Textbox, HelpText = "Maximum time to retry Emergency alerts, maximum 10800 seconds")]
        public int Expire { get; set; }

        [FieldDefinition(5, Label = "Sound", Type = FieldType.Textbox, HelpText = "Notification sound 0-62 (leave blank to use the default)", HelpLink = "https://www.pushsafer.com/en/pushapi_ext#API-S")]
        public string Sound { get; set; }

        [FieldDefinition(6, Label = "Vibration", Type = FieldType.Textbox, HelpText = "Vibration pattern 1-3 (leave blank to use the device default)", HelpLink = "https://www.pushsafer.com/en/pushapi_ext#API-V")]
        public string Vibration { get; set; }

        [FieldDefinition(7, Label = "Icon", Type = FieldType.Textbox, HelpText = "Icon number 1-181 (leave blank to use the default Pushsafer icon)", HelpLink = "https://www.pushsafer.com/en/pushapi_ext#API-I")]
        public string Icon { get; set; }

        [FieldDefinition(8, Label = "Icon Color", Type = FieldType.Textbox, HelpText = "Icon color in hex format (leave blank to use the default Pushsafer icon color)", HelpLink = "https://www.pushsafer.com/en/pushapi_ext#API-C")]
        public string IconColor { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
