﻿using FluentValidation;
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

        [FieldDefinition(2, Type = FieldType.Checkbox, Label = "Prefer Golden", HelpText = "Favors Golden Popcorn-releases over all other releases.")]
        public bool Golden { get; set; }

        [FieldDefinition(3, Type = FieldType.Checkbox, Label = "Prefer Scene", HelpText = "Favors scene-releases over non-scene releases.")]
        public bool Scene { get; set; }

        [FieldDefinition(4, Type = FieldType.Checkbox, Label = "Require Approved", HelpText = "Require staff-approval for releases to be accepted.")]
        public bool Approved { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
