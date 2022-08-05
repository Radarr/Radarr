using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Ntfy
{
    public class NtfySettingsValidator : AbstractValidator<NtfySettings>
    {
        public NtfySettingsValidator()
        {
            RuleFor(c => c.Topics).NotEmpty();
            RuleFor(c => c.Priority).InclusiveBetween(1, 5);
            RuleFor(c => c.Title).NotEmpty();
            RuleFor(c => c.ServerUrl).IsValidUrl().When(c => !c.ServerUrl.IsNullOrWhiteSpace());
            RuleFor(c => c.ClickUrl).IsValidUrl().When(c => !c.ClickUrl.IsNullOrWhiteSpace());
            RuleForEach(c => c.Topics).NotEmpty().Matches("[a-zA-Z0-9_-]+").Must(c => !InvalidTopics.Contains(c)).WithMessage("Invalid topic");
        }

        private static List<string> InvalidTopics => new List<string> { "announcements", "app", "docs", "settings", "stats", "mytopic-rw", "mytopic-ro", "mytopic-wo" };
    }

    public class NtfySettings : IProviderConfig
    {
        private static readonly NtfySettingsValidator Validator = new NtfySettingsValidator();

        public NtfySettings()
        {
            Topics = Array.Empty<string>();
            Priority = 3;
        }

        [FieldDefinition(0, Label = "Server Url", Type = FieldType.Url, HelpLink = "https://ntfy.sh", HelpText = "Leave blank for https://ntfy.sh")]
        public string ServerUrl { get; set; }

        [FieldDefinition(1, Label = "User Name", HelpText = "Optional Authorisation", Privacy = PrivacyLevel.UserName)]
        public string UserName { get; set; }

        [FieldDefinition(2, Label = "Password", Type = FieldType.Password, HelpText = "Optional Password", Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "Message Title", Placeholder = "Radarr alert")]
        public string Title { get; set; }

        [FieldDefinition(5, Label = "Priority", Type = FieldType.Select, SelectOptions = typeof(NtfyPriority))]
        public int Priority { get; set; }

        [FieldDefinition(6, Label = "Topics", HelpText = "List of Topics to send notifications to", Type = FieldType.Tag, Placeholder = "Topic1234,Topic4321")]
        public IEnumerable<string> Topics { get; set; }

        [FieldDefinition(7, Label = "Tags", Type = FieldType.Tag, HelpText = "Optional list of tags to use", Placeholder = "warning,skull", HelpLink = "https://ntfy.sh/docs/emojis/")]
        public IEnumerable<string> Tags { get; set; }

        [FieldDefinition(8, Label = "Click Url", Type = FieldType.Url, HelpText = "Optional url to direct user to on alert", Placeholder = "https://radarr.io")]
        public string ClickUrl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
