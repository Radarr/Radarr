using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.DiscordNotifier
{
    public class DiscordNotifierSettingsValidator : AbstractValidator<DiscordNotifierSettings>
    {
        public DiscordNotifierSettingsValidator()
        {
            RuleFor(c => c.APIKey).NotEmpty();
        }
    }

    public class DiscordNotifierSettings : IProviderConfig
    {
        private static readonly DiscordNotifierSettingsValidator Validator = new DiscordNotifierSettingsValidator();

        [FieldDefinition(0, Label = "API Key", HelpText = "Your API key from your profile", HelpLink = "https://discordnotifier.com")]
        public string APIKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
