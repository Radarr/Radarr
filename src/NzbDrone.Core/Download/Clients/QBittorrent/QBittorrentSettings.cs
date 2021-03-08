using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Download.Clients.QBittorrent
{
    public class QBittorrentSettingsValidator : AbstractValidator<QBittorrentSettings>
    {
        public QBittorrentSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.UrlBase).ValidUrlBase().When(c => c.UrlBase.IsNotNullOrWhiteSpace());

            RuleFor(c => c.MovieCategory).Matches(@"^([^\\\/](\/?[^\\\/])*)?$").WithMessage(@"Can not contain '\', '//', or start/end with '/'");
            RuleFor(c => c.MovieImportedCategory).Matches(@"^([^\\\/](\/?[^\\\/])*)?$").WithMessage(@"Can not contain '\', '//', or start/end with '/'");
        }
    }

    public class QBittorrentSettings : IProviderConfig
    {
        private static readonly QBittorrentSettingsValidator Validator = new QBittorrentSettingsValidator();

        public QBittorrentSettings()
        {
            Host = "localhost";
            Port = 8080;
            MovieCategory = "radarr";
        }

        [FieldDefinition(0, Label = "Host", Type = FieldType.Textbox)]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port", Type = FieldType.Textbox)]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Use a secure connection. See Options -> Web UI -> 'Use HTTPS instead of HTTP' in qBittorrent.")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Url Base", Type = FieldType.Textbox, Advanced = true, HelpText = "Adds a prefix to the qBittorrent url, e.g. http://[host]:[port]/[urlBase]/api")]
        public string UrlBase { get; set; }

        [FieldDefinition(4, Label = "Username", Type = FieldType.Textbox, Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(5, Label = "Password", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string Password { get; set; }

        [FieldDefinition(6, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Radarr avoids conflicts with unrelated downloads, but it's optional")]
        public string MovieCategory { get; set; }

        [FieldDefinition(7, Label = "Post-Import Category", Type = FieldType.Textbox, Advanced = true, HelpText = "Category for Radarr to set after it has imported the download. Radarr will not remove the torrent if seeding has finished. Leave blank to keep same category.")]
        public string MovieImportedCategory { get; set; }

        [FieldDefinition(8, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(QBittorrentPriority), HelpText = "Priority to use when grabbing movies that released within the last 14 days")]
        public int RecentMoviePriority { get; set; }

        [FieldDefinition(9, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(QBittorrentPriority), HelpText = "Priority to use when grabbing movies that were released over 14 days ago")]
        public int OlderMoviePriority { get; set; }

        [FieldDefinition(10, Label = "Initial State", Type = FieldType.Select, SelectOptions = typeof(QBittorrentState), HelpText = "Initial state for torrents added to qBittorrent")]
        public int InitialState { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
