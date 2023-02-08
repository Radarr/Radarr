using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Apprise
{
    public class AppriseSettingsValidator : AbstractValidator<AppriseSettings>
    {
        public AppriseSettingsValidator()
        {
            RuleFor(c => c.NotificationType).InclusiveBetween(0, 3);
            RuleFor(c => c.ServerUrl).IsValidUrl().When(c => !c.ServerUrl.IsNullOrWhiteSpace());
        }
    }

    public class AppriseSettings : IProviderConfig
    {
        private static readonly AppriseSettingsValidator Validator = new AppriseSettingsValidator();

        public AppriseSettings()
        {
            NotificationType = 0;
            Tag = null;
        }

        [FieldDefinition(0, Label = "Server Url", Type = FieldType.Url, HelpLink = "https://github.com/caronc/apprise-api#api-details", HelpText = "Use http or https scheme", Placeholder = "http://localhost:8000/notify/apprise")]
        public string ServerUrl { get; set; }

        [FieldDefinition(3, Label = "Notification Type", Type = FieldType.Select, SelectOptions = typeof(ApprisePriority))]
        public int NotificationType { get; set; }

        [FieldDefinition(5, Label = "Apprise Tag", Type = FieldType.Tag, HelpText = "Optional tag to send", Placeholder = "all", HelpLink = "https://github.com/caronc/apprise/wiki/config")]
        public string Tag { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
