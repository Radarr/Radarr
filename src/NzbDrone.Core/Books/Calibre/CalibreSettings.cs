using System;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
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

    public class CalibreSettings : IEmbeddedDocument
    {
        private static readonly CalibreSettingsValidator Validator = new CalibreSettingsValidator();

        public CalibreSettings()
        {
            Port = 8080;
        }

        public string Host { get; set; }
        public int Port { get; set; }
        public string UrlBase { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Library { get; set; }
        public string OutputFormat { get; set; }
        public int OutputProfile { get; set; }
        public bool UseSsl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
