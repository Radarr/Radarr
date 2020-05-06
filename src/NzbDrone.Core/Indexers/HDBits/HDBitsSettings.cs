using System.Collections.Generic;
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

    public class HDBitsSettings : ITorrentIndexerSettings
    {
        private static readonly HDBitsSettingsValidator Validator = new HDBitsSettingsValidator();

        public HDBitsSettings()
        {
            BaseUrl = "https://hdbits.org";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;

            Categories = new int[] { (int)HdBitsCategory.Movie };
            Codecs = new int[0];
            Mediums = new int[0];
            MultiLanguages = new List<int>();
            RequiredFlags = new List<int>();
        }

        [FieldDefinition(0, Label = "Username")]
        public string Username { get; set; }

        [FieldDefinition(1, Type = FieldType.TagSelect, SelectOptions = typeof(LanguageFieldConverter), Label = "Multi Languages", HelpText = "What languages are normally in a multi release on this indexer?", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(2, Label = "API Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(3, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your API key will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(4, Label = "Categories", Type = FieldType.Tag, SelectOptions = typeof(HdBitsCategory), Advanced = true, HelpText = "Options: Movie, TV, Documentary, Music, Sport, Audio, XXX, MiscDemo. If unspecified, all options are used.")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(5, Label = "Codecs", Type = FieldType.Tag, SelectOptions = typeof(HdBitsCodec), Advanced = true, HelpText = "Options: h264, Mpeg2, VC1, Xvid. If unspecified, all options are used.")]
        public IEnumerable<int> Codecs { get; set; }

        [FieldDefinition(6, Label = "Mediums", Type = FieldType.Tag, SelectOptions = typeof(HdBitsMedium), Advanced = true, HelpText = "Options: BluRay, Encode, Capture, Remux, WebDL. If unspecified, all options are used.")]
        public IEnumerable<int> Mediums { get; set; }

        [FieldDefinition(7, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(8, Type = FieldType.TagSelect, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", HelpLink = "https://github.com/Radarr/Radarr/wiki/Indexer-Flags#1-required-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        [FieldDefinition(9)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new SeedCriteriaSettings();

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum HdBitsCategory
    {
        Movie = 1,
        Tv = 2,
        Documentary = 3,
        Music = 4,
        Sport = 5,
        Audio = 6,
        Xxx = 7,
        MiscDemo = 8
    }

    public enum HdBitsCodec
    {
        H264 = 1,
        Mpeg2 = 2,
        Vc1 = 3,
        Xvid = 4,
        HEVC = 5
    }

    public enum HdBitsMedium
    {
        Bluray = 1,
        Encode = 3,
        Capture = 4,
        Remux = 5,
        WebDl = 6
    }
}
