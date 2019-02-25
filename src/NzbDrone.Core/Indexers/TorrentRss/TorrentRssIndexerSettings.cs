using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.TorrentRss
{
    public class TorrentRssIndexerSettingsValidator : AbstractValidator<TorrentRssIndexerSettings>
    {
        public TorrentRssIndexerSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class TorrentRssIndexerSettings : ITorrentIndexerSettings
    {
        private static readonly TorrentRssIndexerSettingsValidator validator = new TorrentRssIndexerSettingsValidator();

        public TorrentRssIndexerSettings()
        {
            BaseUrl = string.Empty;
            AllowZeroSize = false;
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
        }

        [FieldDefinition(0, Label = "Full RSS Feed URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Cookie", HelpText = "If you site requires a login cookie to access the rss, you'll have to retrieve it via a browser.")]
        public string Cookie { get; set; }

        [FieldDefinition(2, Type = FieldType.Checkbox, Label = "Allow Zero Size", HelpText="Enabling this will allow you to use feeds that don't specify release size, but be careful, size related checks will not be performed.")]
        public bool AllowZeroSize { get; set; }
                
        [FieldDefinition(3, Type = FieldType.Tag, SelectOptions = typeof(Language), Label = "Multi Languages", HelpText = "What languages are normally in a multi release on this indexer?", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(4, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(5, Type = FieldType.Tag, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", HelpLink = "https://github.com/Radarr/Radarr/wiki/Indexer-Flags#1-required-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(validator.Validate(this));
        }
    }
}
