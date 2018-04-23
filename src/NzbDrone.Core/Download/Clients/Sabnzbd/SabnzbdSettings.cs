using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.Sabnzbd
{
    public class SabnzbdSettingsValidator : AbstractValidator<SabnzbdSettings>
    {
        public SabnzbdSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);

            RuleFor(c => c.ApiKey).NotEmpty()
                                  .WithMessage("API Key is required when username/password are not configured")
                                  .When(c => string.IsNullOrWhiteSpace(c.Username));

            RuleFor(c => c.Username).NotEmpty()
                                    .WithMessage("Username is required when API key is not configured")
                                    .When(c => string.IsNullOrWhiteSpace(c.ApiKey));

            RuleFor(c => c.Password).NotEmpty()
                                    .WithMessage("Password is required when API key is not configured")
                                    .When(c => string.IsNullOrWhiteSpace(c.ApiKey));

            RuleFor(c => c.MovieCategory).NotEmpty()
                                      .WithMessage("A category is recommended")
                                      .AsWarning();
        }
    }

    public class SabnzbdSettings : IProviderConfig
    {
        private static readonly SabnzbdSettingsValidator Validator = new SabnzbdSettingsValidator();

        public SabnzbdSettings()
        {
            Host = "localhost";
            Port = 8080;
            MovieCategory = "movies";
            RecentMoviePriority = (int)SabnzbdPriority.Default;
            OlderMoviePriority = (int)SabnzbdPriority.Default;
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "API Key", Type = FieldType.Textbox)]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "Username", Type = FieldType.Textbox)]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "Password", Type = FieldType.Password)]
        public string Password { get; set; }

        [FieldDefinition(5, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Radarr avoids conflicts with unrelated downloads, but it's optional")]
        public string MovieCategory { get; set; }

        [FieldDefinition(6, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(SabnzbdPriority), HelpText = "Priority to use when grabbing movies that released within the last 21 days")]
        public int RecentMoviePriority { get; set; }

        [FieldDefinition(7, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(SabnzbdPriority), HelpText = "Priority to use when grabbing movies that released over 21 days ago")]
        public int OlderMoviePriority { get; set; }

        [FieldDefinition(8, Label = "Use SSL", Type = FieldType.Checkbox)]
        public bool UseSsl { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
