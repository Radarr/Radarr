using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.RQbit
{
    public class RQbitSettings : DownloadClientSettingsBase<RQbitSettings>
    {
        private static readonly RQbitSettingsValidator Validator = new RQbitSettingsValidator();

        public RQbitSettings()
        {
            Host = "localhost";
            Port = 3030;
            UrlBase = "/";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "UseSsl", Type = FieldType.Checkbox, HelpText = "DownloadClientQbittorrentSettingsUseSslHelpText")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "UrlBase", Type = FieldType.Textbox, Advanced = true, HelpText = "DownloadClientSettingsUrlBaseHelpText")]
        [FieldToken(TokenField.HelpText, "UrlBase", "clientName", "RQBit")]
        [FieldToken(TokenField.HelpText, "UrlBase", "url", "http://[host]:[port]/")]
        public string UrlBase { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
