using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Transmission
{
    public class TransmissionSettingsValidator : AbstractValidator<TransmissionSettings>
    {
        public TransmissionSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);

            RuleFor(c => c.UrlBase).ValidUrlBase();

            RuleFor(c => c.MovieCategory).Matches(@"^\.?[-a-z]*$", RegexOptions.IgnoreCase).WithMessage("Allowed characters a-z and -");

            RuleFor(c => c.MovieCategory).Empty()
                .When(c => c.MovieDirectory.IsNotNullOrWhiteSpace())
                .WithMessage("Cannot use Category and Directory");
        }
    }

    public class TransmissionSettings : IProviderConfig
    {
        private static readonly TransmissionSettingsValidator Validator = new TransmissionSettingsValidator();

        public TransmissionSettings()
        {
            Host = "localhost";
            Port = 9091;
            UrlBase = "/transmission/";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Url Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the transmission rpc url, eg http://[host]:[port]/[urlBase]/rpc, defaults to '/transmission/'")]
        public string UrlBase { get; set; }

        [FieldDefinition(3, Label = "Username", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(5, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Radarr avoids conflicts with unrelated downloads, but it's optional. Creates a [category] subdirectory in the output directory.")]
        public string MovieCategory { get; set; }

        [FieldDefinition(6, Label = "Directory", Type = FieldType.Textbox, Advanced = true, HelpText = "Optional location to put downloads in, leave blank to use the default Transmission location")]
        public string MovieDirectory { get; set; }

        [FieldDefinition(7, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(TransmissionPriority), HelpText = "Priority to use when grabbing movies that released within the last 21 days")]
        public int RecentMoviePriority { get; set; }

        [FieldDefinition(8, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(TransmissionPriority), HelpText = "Priority to use when grabbing movies that released over 21 days ago")]
        public int OlderMoviePriority { get; set; }

        [FieldDefinition(9, Label = "Add Paused", Type = FieldType.Checkbox)]
        public bool AddPaused { get; set; }

        [FieldDefinition(10, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
