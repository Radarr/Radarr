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
            StalledThreshold = 604800; // a week
            StalledInactivityThreshold = 86400; // a day
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

        [FieldDefinition(6, Label = "Category", Type = FieldType.Textbox, HelpText = "Adding a category specific to Radarr avoids conflicts with unrelated non-Radarr downloads. Using a category is optional, but strongly recommended.")]
        public string MovieCategory { get; set; }

        [FieldDefinition(7, Label = "Post-Import Category", Type = FieldType.Textbox, Advanced = true, HelpText = "Category for Radarr to set after it has imported the download. Radarr will not remove the torrent if seeding has finished. Leave blank to keep same category.")]
        public string MovieImportedCategory { get; set; }

        [FieldDefinition(8, Label = "Recent Priority", Type = FieldType.Select, SelectOptions = typeof(QBittorrentPriority), HelpText = "Priority to use when grabbing movies that released within the last 14 days")]
        public int RecentMoviePriority { get; set; }

        [FieldDefinition(9, Label = "Older Priority", Type = FieldType.Select, SelectOptions = typeof(QBittorrentPriority), HelpText = "Priority to use when grabbing movies that were released over 14 days ago")]
        public int OlderMoviePriority { get; set; }

        [FieldDefinition(10, Label = "Initial State", Type = FieldType.Select, SelectOptions = typeof(QBittorrentState), HelpText = "Initial state for torrents added to qBittorrent. Note that Forced Torrents do not abide by seed restrictions")]
        public int InitialState { get; set; }

        [FieldDefinition(11, Label = "Sequential Order", Type = FieldType.Checkbox, HelpText = "Download in sequential order (qBittorrent 4.1.0+)")]
        public bool SequentialOrder { get; set; }

        [FieldDefinition(12, Label = "First and Last First", Type = FieldType.Checkbox, HelpText = "Download first and last pieces first (qBittorrent 4.1.0+)")]
        public bool FirstAndLast { get; set; }

        [FieldDefinition(13, Label = "Remove stalled downloads", Type = FieldType.Checkbox, Advanced = true, HelpText = "Remove stalled downloads from the client. BEWARE: This could get you banned from trackers!")]
        public bool RemoveStalledDownloads { get; set; }

        [FieldDefinition(14, Label = "Stalled age threshold", Type = FieldType.Textbox, Advanced = true, Unit = "Minute", HelpText = "Age threshold before a stalled torrent is considered for deletion")]
        public long StalledThreshold { get; set; }

        [FieldDefinition(15, Label = "Stalled inactivity threshold", Type = FieldType.Textbox, Advanced = true, Unit = "Minute", HelpText = "Inactivity threshold before a stalled torrent is considered for deletion")]
        public long StalledInactivityThreshold { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
