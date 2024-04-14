using System;
using System.Collections.Generic;
using Equ;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.FileList
{
    public class FileListSettingsValidator : AbstractValidator<FileListSettings>
    {
        public FileListSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Passkey).NotEmpty();

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class FileListSettings : PropertywiseEquatable<FileListSettings>, ITorrentIndexerSettings
    {
        private static readonly FileListSettingsValidator Validator = new ();

        public FileListSettings()
        {
            BaseUrl = "https://filelist.io";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;

            Categories = new[]
            {
                (int)FileListCategories.Movie_HD,
                (int)FileListCategories.Movie_SD,
                (int)FileListCategories.Movie_4K
            };

            MultiLanguages = Array.Empty<int>();
            RequiredFlags = Array.Empty<int>();
        }

        [FieldDefinition(0, Label = "Username", Privacy = PrivacyLevel.UserName)]
        public string Username { get; set; }

        [FieldDefinition(1, Label = "Passkey", Privacy = PrivacyLevel.ApiKey)]
        public string Passkey { get; set; }

        [FieldDefinition(2, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your API key will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(3, Label = "Categories", Type = FieldType.Select, SelectOptions = typeof(FileListCategories), HelpText = "Categories for use in search and feeds. If unspecified, all options are used.")]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(4, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(5)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new ();

        [FieldDefinition(6, Type = FieldType.Checkbox, Label = "IndexerSettingsRejectBlocklistedTorrentHashes", HelpText = "IndexerSettingsRejectBlocklistedTorrentHashesHelpText", Advanced = true)]
        public bool RejectBlocklistedTorrentHashesWhileGrabbing { get; set; }

        [FieldDefinition(7, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "IndexerSettingsMultiLanguageRelease", HelpText = "IndexerSettingsMultiLanguageReleaseHelpText", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(8, Type = FieldType.Select, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", HelpLink = "https://wiki.servarr.com/radarr/settings#indexer-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

    public enum FileListCategories
    {
        [FieldOption("Movies SD")]
        Movie_SD = 1,
        [FieldOption("Movies DVD")]
        Movie_DVD = 2,
        [FieldOption("Movies DVD-RO")]
        Movie_DVDRO = 3,
        [FieldOption("Movies HD")]
        Movie_HD = 4,
        [FieldOption("Movies HD-RO")]
        Movie_HDRO = 19,
        [FieldOption("Movies 4K")]
        Movie_4K = 6,
        [FieldOption("Movies Blu-Ray")]
        Movie_BluRay = 20,
        [FieldOption("Movies 4K Blu-Ray")]
        Movie_BluRay4K = 26,
        [FieldOption("Movies 3D")]
        Movie_3D = 25,
        [FieldOption("XXX")]
        Xxx = 7
    }
}
