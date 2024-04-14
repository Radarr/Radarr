using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.PushBullet
{
    public class PushBulletSettingsValidator : AbstractValidator<PushBulletSettings>
    {
        public PushBulletSettingsValidator()
        {
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class PushBulletSettings : NotificationSettingsBase<PushBulletSettings>
    {
        private static readonly PushBulletSettingsValidator Validator = new ();

        public PushBulletSettings()
        {
            DeviceIds = Array.Empty<string>();
            ChannelTags = Array.Empty<string>();
        }

        [FieldDefinition(0, Label = "NotificationsPushBulletSettingsAccessToken", Privacy = PrivacyLevel.ApiKey, HelpLink = "https://www.pushbullet.com/#settings/account")]
        public string ApiKey { get; set; }

        [FieldDefinition(1, Label = "NotificationsPushBulletSettingsDeviceIds", HelpText = "NotificationsPushBulletSettingsDeviceIdsHelpText", Type = FieldType.Device, Placeholder = "123456789,987654321")]
        public IEnumerable<string> DeviceIds { get; set; }

        [FieldDefinition(2, Label = "NotificationsPushBulletSettingsChannelTags", HelpText = "NotificationsPushBulletSettingsChannelTagsHelpText", Type = FieldType.Tag, Placeholder = "Channel1234,Channel4321")]
        public IEnumerable<string> ChannelTags { get; set; }

        [FieldDefinition(3, Label = "NotificationsPushBulletSettingSenderId", HelpText = "NotificationsPushBulletSettingSenderIdHelpText")]
        public string SenderId { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
