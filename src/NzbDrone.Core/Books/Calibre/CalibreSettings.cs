using System;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Books.Calibre
{
    public class CalibreSettingsValidator : AbstractValidator<CalibreSettings>
    {
        public CalibreSettingsValidator()
        {
            RuleFor(c => c.Host).IsValidUrl();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.UrlBase).ValidUrlBase().When(c => c.UrlBase.IsNotNullOrWhiteSpace());

            RuleFor(c => c.Username).NotEmpty().When(c => !string.IsNullOrWhiteSpace(c.Password));
            RuleFor(c => c.Password).NotEmpty().When(c => !string.IsNullOrWhiteSpace(c.Username));

            RuleFor(c => c.OutputFormat).Must(x => x.Split(',').All(y => Enum.TryParse<CalibreFormat>(y, true, out _))).WithMessage("Invalid output formats");
        }
    }

    public class CalibreSettings : IProviderConfig
    {
        private static readonly CalibreSettingsValidator Validator = new CalibreSettingsValidator();

        public CalibreSettings()
        {
            Port = 8080;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Url Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the calibre url, e.g. http://[host]:[port]/[urlBase]")]
        public string UrlBase { get; set; }

        [FieldDefinition(3, Label = "Username", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(5, Label = "Convert to Format", Type = FieldType.Textbox, HelpText = "Optionally ask calibre to convert to other formats on import. Comma separated list.")]
        public string OutputFormat { get; set; }

        [FieldDefinition(6, Label = "Conversion Profile", Type = FieldType.Select, SelectOptions = typeof(CalibreProfile), HelpText = "The output profile to use for conversion")]
        public int OutputProfile { get; set; }

        [FieldDefinition(9, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
