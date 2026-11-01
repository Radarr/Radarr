using FluentValidation;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Tdarr
{
    public class TdarrSettingsValidator : AbstractValidator<TdarrSettings>
    {
        public TdarrSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).GreaterThan(0);
            RuleFor(c => c.UrlBase).ValidUrlBase();
            RuleFor(c => c.LibraryId).NotEmpty();
            RuleFor(c => c.MapFrom).NotEmpty().Unless(c => c.MapTo.IsNullOrWhiteSpace());
            RuleFor(c => c.MapTo).NotEmpty().Unless(c => c.MapFrom.IsNullOrWhiteSpace());
        }
    }

    public class TdarrSettings : NotificationSettingsBase<TdarrSettings>
    {
        private static readonly TdarrSettingsValidator Validator = new();

        public TdarrSettings()
        {
            Port = 8265;
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "NotificationsSettingsUseSslHelpText")]
        [FieldToken(TokenField.HelpText, "UseSsl", "serviceName", "TDarr")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "UrlBase", Type = FieldType.Textbox, Advanced = true, HelpText = "ConnectionSettingsUrlBaseHelpText")]
        [FieldToken(TokenField.HelpText, "UrlBase", "connectionName", "TDarr")]
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "http://[host]:[port]/[urlBase]")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "NotificationsTdarrSettingsApiKey", Privacy = PrivacyLevel.ApiKey, HelpText = "NotificationsTdarrSettingsApiKeyHelpText")]
        public string ApiKey { get; set; }

        [FieldDefinition(5, Label = "NotificationsTdarrSettingsLibraryId", HelpText = "NotificationsTdarrSettingsLibraryIdHelpText")]
        public string LibraryId { get; set; }

        [FieldDefinition(6, Label = "NotificationsSettingsUpdateMapPathsFrom", HelpText = "NotificationsSettingsUpdateMapPathsFromMovieHelpText", Type = FieldType.Textbox, Advanced = true)]
        [FieldToken(TokenField.HelpText, "NotificationsSettingsUpdateMapPathsFrom", "serviceName", "TDarr")]
        public string MapFrom { get; set; }

        [FieldDefinition(7, Label = "NotificationsSettingsUpdateMapPathsTo", HelpText = "NotificationsSettingsUpdateMapPathsToMovieHelpText", Type = FieldType.Textbox, Advanced = true)]
        [FieldToken(TokenField.HelpText, "NotificationsSettingsUpdateMapPathsTo", "serviceName", "TDarr")]
        public string MapTo { get; set; }

        [JsonIgnore]
        public string Address => $"{Host.ToUrlHost()}:{Port}{UrlBase}";

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
