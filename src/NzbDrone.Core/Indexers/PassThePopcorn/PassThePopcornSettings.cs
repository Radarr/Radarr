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
            RuleFor(c => c.Cookie).NotEmpty();

            RuleFor(c => c.Cookie)
                .Matches(@"__cfduid=[0-9a-f]{43}", RegexOptions.IgnoreCase)
                .WithMessage("Wrong pattern")
                .AsWarning();
        }
    }

    public class PassThePopcornSettings : IProviderConfig
    {
        private static readonly PassThePopcornSettingsValidator Validator = new PassThePopcornSettingsValidator();

        public PassThePopcornSettings()
        {
            BaseUrl = "https://passthepopcorn.me";
        }

        [FieldDefinition(0, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your cookie will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Cookie", HelpText = "PassThePopcorn uses a login cookie needed to access the API, you'll have to retrieve it via a browser.")]
        public string Cookie { get; set; }

        [FieldDefinition(2, Type = FieldType.Checkbox, Label = "Only Golden", HelpText = "Only include golden torrents.")]
        public bool GoldenOnly { get; set; }

        [FieldDefinition(3, Type = FieldType.Checkbox, Label = "Ony Staff Approved", HelpText = "Only include staff approved torrents.")]
        public bool CheckedOnly { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
