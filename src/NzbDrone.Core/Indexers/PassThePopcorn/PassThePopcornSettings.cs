using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornSettingsValidator : AbstractValidator<PassThePopcornSettings>
    {
        public PassThePopcornSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.APIUser).NotEmpty();
            RuleFor(c => c.APIKey).NotEmpty();

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class PassThePopcornSettings : ITorrentIndexerSettings
    {
        private static readonly PassThePopcornSettingsValidator Validator = new ();

        public PassThePopcornSettings()
        {
            BaseUrl = "https://passthepopcorn.me";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
            MultiLanguages = Array.Empty<int>();
            RequiredFlags = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your cookie will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "APIUser", HelpText = "These settings are found in your PassThePopcorn security settings (Edit Profile > Security).", Privacy = PrivacyLevel.UserName)]
        public string APIUser { get; set; }

        [FieldDefinition(2, Label = "APIKey", Type = FieldType.Password, Privacy = PrivacyLevel.Password)]
        public string APIKey { get; set; }

        [FieldDefinition(3, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "Multi Languages", HelpText = "What languages are normally in a multi release on this indexer?", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(4, Type = FieldType.Textbox, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(5, Type = FieldType.Select, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", HelpLink = "https://wiki.servarr.com/radarr/settings#indexer-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        [FieldDefinition(6)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new SeedCriteriaSettings();

        [FieldDefinition(7, Type = FieldType.Checkbox, Label = "Reject Blocklisted Torrent Hashes While Grabbing", HelpText = "If a torrent is blocked by hash it may not properly be rejected during RSS/Search for some indexers, enabling this will allow it to be rejected after the torrent is grabbed, but before it is sent to the client.", Advanced = true)]
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
