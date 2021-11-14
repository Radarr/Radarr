using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Email
{
    public class EmailSettingsValidator : AbstractValidator<EmailSettings>
    {
        public EmailSettingsValidator()
        {
            RuleFor(c => c.Server).NotEmpty();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.From).NotEmpty();
            RuleForEach(c => c.To).EmailAddress();
            RuleForEach(c => c.CC).EmailAddress();
            RuleForEach(c => c.Bcc).EmailAddress();

            // Only require one of three send fields to be set
            RuleFor(c => c.To).NotEmpty().Unless(c => c.Bcc.Any() || c.CC.Any());
            RuleFor(c => c.CC).NotEmpty().Unless(c => c.To.Any() || c.Bcc.Any());
            RuleFor(c => c.Bcc).NotEmpty().Unless(c => c.To.Any() || c.CC.Any());
        }
    }

    public class EmailSettings : IProviderConfig
    {
        private static readonly EmailSettingsValidator Validator = new EmailSettingsValidator();

        public EmailSettings()
        {
            Port = 567;
            To = Array.Empty<string>();
            CC = Array.Empty<string>();
            Bcc = Array.Empty<string>();
        }

        [FieldDefinition(0, Label = "Server", HelpText = "Hostname or IP of Email server", Placeholder = "smtp.gmail.com")]
        public string Server { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Require Encryption", HelpText = "Require SSL (Port 465 only) or StartTLS (any other port)", Type = FieldType.Checkbox)]
        public bool RequireEncryption { get; set; }

        [FieldDefinition(3, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(5, Label = "From Address", Placeholder = "example@email.com")]
        public string From { get; set; }

        [FieldDefinition(6, Label = "Recipient Address(es)", HelpText = "Comma separated list of email recipients", Placeholder = "example@email.com,example1@email.com")]
        public IEnumerable<string> To { get; set; }

        [FieldDefinition(7, Label = "CC Address(es)", HelpText = "Comma separated list of email cc recipients", Placeholder = "example@email.com,example1@email.com", Advanced = true)]
        public IEnumerable<string> CC { get; set; }

        [FieldDefinition(8, Label = "BCC Address(es)", HelpText = "Comma separated list of email bcc recipients", Placeholder = "example@email.com,example1@email.com", Advanced = true)]
        public IEnumerable<string> Bcc { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
