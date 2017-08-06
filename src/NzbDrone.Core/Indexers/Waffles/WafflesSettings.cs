using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Waffles
{
    public class WafflesSettingsValidator : AbstractValidator<WafflesSettings>
    {
        public WafflesSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.UserId).NotEmpty();
            RuleFor(c => c.RssPasskey).NotEmpty();
        }
    }

    public class WafflesSettings : IProviderConfig
    {
        private static readonly WafflesSettingsValidator Validator = new WafflesSettingsValidator();

        public WafflesSettings()
        {
            BaseUrl = "https://www.waffles.ch";
        }

        [FieldDefinition(0, Label = "Website URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "UserId")]
        public string UserId { get; set; }

        [FieldDefinition(2, Label = "RSS Passkey")]
        public string RssPasskey { get; set; }


        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}