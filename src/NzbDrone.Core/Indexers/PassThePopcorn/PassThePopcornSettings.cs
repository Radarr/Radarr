using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using System.Text.RegularExpressions;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornSettingsValidator : AbstractValidator<PassThePopcornSettings>
    {
        public PassThePopcornSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Password).NotEmpty();
            RuleFor(c => c.Passkey).NotEmpty();
        }
    }

    public class PassThePopcornSettings : IProviderConfig
    {
        private static readonly PassThePopcornSettingsValidator Validator = new PassThePopcornSettingsValidator();

        public PassThePopcornSettings()
        {
            BaseUrl = "https://passthepopcorn.me";
        }

        [FieldDefinition(0, Label = "URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your cookie will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Username", HelpText = "PTP Username")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "Password", Type = FieldType.Password, HelpText = "PTP Password")]
        public string Password { get; set; }

        [FieldDefinition(3, Label = "Passkey",  HelpText = "PTP Passkey")]
        public string Passkey { get; set; }

        [FieldDefinition(4, Label = "Prefer Golden", Type = FieldType.Checkbox , HelpText = "Favors Golden Popcorn-releases over all other releases.")]
        public bool Golden { get; set; }

        [FieldDefinition(5, Label = "Prefer Scene", Type = FieldType.Checkbox, HelpText = "Favors scene-releases over non-scene releases.")]
        public bool Scene { get; set; }

        [FieldDefinition(6, Label = "Require Approved", Type = FieldType.Checkbox, HelpText = "Require staff-approval for releases to be accepted.")]
        public bool Approved { get; set; }

        [FieldDefinition(7, Label = "Require Golden", Type = FieldType.Checkbox, HelpText = "Require Golden Popcorn-releases for releases to be accepted.")]
        public bool RequireGolden { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
