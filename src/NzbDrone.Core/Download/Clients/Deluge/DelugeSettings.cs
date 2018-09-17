using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Deluge
{
    public class DelugeSettingsValidator : AbstractValidator<DelugeSettings>
    {
        public DelugeSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);

            RuleFor(c => c.MovieCategory).Matches("^[-a-z]*$").WithMessage("Allowed characters a-z and -");
        }
    }

    public class DelugeSettings : IProviderConfig
    {
        private static readonly DelugeSettingsValidator Validator = new DelugeSettingsValidator();

        public DelugeSettings()
        {
            Host = "localhost";
            Port = 8112;
            Password = "deluge";
            MovieCategory = "radarr";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Url Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the deluge json url, see http://[host]:[port]/[urlBase]/json")]
        public string UrlBase { get; set; }

        [FieldDefinition(3, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(4, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Radarr avoids conflicts with unrelated downloads, but it's optional")]
        public string MovieCategory { get; set; }

        [FieldDefinition(5, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(DelugePriority), HelpText = "Priority to use when grabbing movies that released within the last 21 days")]
        public int RecentMoviePriority { get; set; }

        [FieldDefinition(6, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(DelugePriority), HelpText = "Priority to use when grabbing movies that released over 21 days ago")]
        public int OlderMoviePriority { get; set; }

        [FieldDefinition(7, Label = "Add Paused", Type = FieldType.Checkbox)]
        public bool AddPaused { get; set; }

        [FieldDefinition(8, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
