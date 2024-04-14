using System;
using System.Collections.Generic;
using Equ;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.HDBits
{
    public class HDBitsSettingsValidator : AbstractValidator<HDBitsSettings>
    {
        public HDBitsSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class HDBitsSettings : PropertywiseEquatable<HDBitsSettings>, ITorrentIndexerSettings
    {
        private static readonly HDBitsSettingsValidator Validator = new ();

        public HDBitsSettings()
        {
            BaseUrl = "https://hdbits.org";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;

            Categories = new[] { (int)HdBitsCategory.Movie };
            Codecs = Array.Empty<int>();
            Mediums = Array.Empty<int>();
            MultiLanguages = Array.Empty<int>();
            RequiredFlags = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your API key will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "API Key", Privacy = PrivacyLevel.ApiKey)]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "Categories", Type = FieldType.Select, SelectOptions = typeof(HdBitsCategory), HelpText = "If unspecified, all options are used.")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(4, Label = "Codecs", Type = FieldType.Select, SelectOptions = typeof(HdBitsCodec), Advanced = true, HelpText = "If unspecified, all options are used.")]
        public IEnumerable<int> Codecs { get; set; }

        [FieldDefinition(5, Label = "Mediums", Type = FieldType.Select, SelectOptions = typeof(HdBitsMedium), Advanced = true, HelpText = "If unspecified, all options are used.")]
        public IEnumerable<int> Mediums { get; set; }

        [FieldDefinition(6, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(7)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new ();

        [FieldDefinition(8, Type = FieldType.Checkbox, Label = "IndexerSettingsRejectBlocklistedTorrentHashes", HelpText = "IndexerSettingsRejectBlocklistedTorrentHashesHelpText", Advanced = true)]
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        [FieldDefinition(9, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "IndexerSettingsMultiLanguageRelease", HelpText = "IndexerSettingsMultiLanguageReleaseHelpText", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(10, Type = FieldType.Select, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", HelpLink = "https://wiki.servarr.com/radarr/settings#indexer-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum HdBitsCategory
    {
        [FieldOption("Movie")]
        Movie = 1,
        [FieldOption("TV")]
        Tv = 2,
        [FieldOption("Documentary")]
        Documentary = 3,
        [FieldOption("Music")]
        Music = 4,
        [FieldOption("Sport")]
        Sport = 5,
        [FieldOption("Audio Track")]
        Audio = 6,
        [FieldOption("XXX")]
        Xxx = 7,
        [FieldOption("Misc/Demo")]
        MiscDemo = 8
    }

    public enum HdBitsCodec
    {
        [FieldOption("H.264")]
        H264 = 1,
        [FieldOption("MPEG-2")]
        Mpeg2 = 2,
        [FieldOption("VC-1")]
        Vc1 = 3,
        [FieldOption("XviD")]
        Xvid = 4,
        [FieldOption("HEVC")]
        HEVC = 5
    }

    public enum HdBitsMedium
    {
        [FieldOption("Blu-ray/HD DVD")]
        Bluray = 1,
        [FieldOption("Encode")]
        Encode = 3,
        [FieldOption("Capture")]
        Capture = 4,
        [FieldOption("Remux")]
        Remux = 5,
        [FieldOption("WEB-DL")]
        WebDl = 6
    }
}
