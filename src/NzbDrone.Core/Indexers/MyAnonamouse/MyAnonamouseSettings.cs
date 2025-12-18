using System;
using System.Collections.Generic;
using Equ;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.MyAnonamouse
{
    public class MyAnonamouseSettingsValidator : AbstractValidator<MyAnonamouseSettings>
    {
        public MyAnonamouseSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.MamId).NotEmpty();
            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class MyAnonamouseSettings : PropertywiseEquatable<MyAnonamouseSettings>, ITorrentIndexerSettings
    {
        private static readonly MyAnonamouseSettingsValidator Validator = new ();

        public MyAnonamouseSettings()
        {
            BaseUrl = "https://www.myanonamouse.net";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
            SearchType = (int)MyAnonamouseSearchType.All;
            SearchInDescription = false;
            SearchInSeries = false;
            SearchInFilenames = false;
            MultiLanguages = Array.Empty<int>();
            FailDownloads = Array.Empty<int>();
            RequiredFlags = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "IndexerSettingsApiUrl", Advanced = true, HelpText = "IndexerSettingsApiUrlHelpText")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "IndexerMyAnonamouseMamId", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey, HelpText = "IndexerMyAnonamouseMamIdHelpText")]
        public string MamId { get; set; }

        [FieldDefinition(2, Label = "IndexerMyAnonamouseSearchType", Type = FieldType.Select, SelectOptions = typeof(MyAnonamouseSearchType), HelpText = "IndexerMyAnonamouseSearchTypeHelpText")]
        public int SearchType { get; set; }

        [FieldDefinition(3, Label = "IndexerMyAnonamouseSearchInDescription", Type = FieldType.Checkbox, HelpText = "IndexerMyAnonamouseSearchInDescriptionHelpText", Advanced = true)]
        public bool SearchInDescription { get; set; }

        [FieldDefinition(4, Label = "IndexerMyAnonamouseSearchInSeries", Type = FieldType.Checkbox, HelpText = "IndexerMyAnonamouseSearchInSeriesHelpText", Advanced = true)]
        public bool SearchInSeries { get; set; }

        [FieldDefinition(5, Label = "IndexerMyAnonamouseSearchInFilenames", Type = FieldType.Checkbox, HelpText = "IndexerMyAnonamouseSearchInFilenamesHelpText", Advanced = true)]
        public bool SearchInFilenames { get; set; }

        [FieldDefinition(6, Type = FieldType.Number, Label = "IndexerSettingsMinimumSeeders", HelpText = "IndexerSettingsMinimumSeedersHelpText", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(7)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new ();

        [FieldDefinition(8, Type = FieldType.Checkbox, Label = "IndexerSettingsRejectBlocklistedTorrentHashes", HelpText = "IndexerSettingsRejectBlocklistedTorrentHashesHelpText", Advanced = true)]
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        [FieldDefinition(9, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "IndexerSettingsMultiLanguageRelease", HelpText = "IndexerSettingsMultiLanguageReleaseHelpText", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(10, Type = FieldType.Select, SelectOptions = typeof(FailDownloads), Label = "IndexerSettingsFailDownloads", HelpText = "IndexerSettingsFailDownloadsHelpText", Advanced = true)]
        public IEnumerable<int> FailDownloads { get; set; }

        [FieldDefinition(11, Type = FieldType.Select, SelectOptions = typeof(IndexerFlags), Label = "IndexerSettingsRequiredFlags", HelpText = "IndexerSettingsRequiredFlagsHelpText", HelpLink = "https://wiki.servarr.com/radarr/settings#indexer-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum MyAnonamouseSearchType
    {
        [FieldOption("All Torrents")]
        All = 0,

        [FieldOption("Only Active")]
        Active = 1,

        [FieldOption("Freeleech")]
        Freeleech = 2,

        [FieldOption("Freeleech or VIP")]
        FreeleechOrVip = 3,

        [FieldOption("VIP")]
        Vip = 4,

        [FieldOption("Not VIP")]
        NotVip = 5
    }
}
