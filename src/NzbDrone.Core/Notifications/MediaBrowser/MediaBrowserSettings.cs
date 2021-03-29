using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Emby
{
    public class MediaBrowserSettingsValidator : AbstractValidator<MediaBrowserSettings>
    {
        public MediaBrowserSettingsValidator()
        {
            RuleFor(c => c.Address).ValidRootUrl();
        }
    }

    public class MediaBrowserSettings : IProviderConfig
    {
        private static readonly MediaBrowserSettingsValidator Validator = new MediaBrowserSettingsValidator();

        public MediaBrowserSettings()
        {
            Address = "http://localhost:8096";
        }

        [FieldDefinition(0, Label = "Address", HelpText = "Emby address with protocol and port, eg: http://localhost:8096")]
        public string Address { get; set; }

        [FieldDefinition(1, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "Send Notifications", HelpText = "Have MediaBrowser send notfications to configured providers", Type = FieldType.Checkbox)]
        public bool Notify { get; set; }

        [FieldDefinition(3, Label = "Update Library", HelpText = "Update Library on Import & Rename?", Type = FieldType.Checkbox)]
        public bool UpdateLibrary { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Address);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
