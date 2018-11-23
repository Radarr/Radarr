using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Rarbg
{
    public class RarbgSettingsValidator : AbstractValidator<RarbgSettings>
    {
        public RarbgSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class RarbgSettings : ITorrentIndexerSettings
    {
        private static readonly RarbgSettingsValidator Validator = new RarbgSettingsValidator();

        public RarbgSettings()
        {
            BaseUrl = "https://torrentapi.org";
            RankedOnly = false;
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
        }

        [FieldDefinition(0, Label = "API URL", HelpText = "URL to Rarbg api, not the website.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Type = FieldType.Checkbox, Label = "Ranked Only", HelpText = "Only include ranked results.")]
        public bool RankedOnly { get; set; }

        [FieldDefinition(2, Type = FieldType.Captcha, Label = "CAPTCHA Token", HelpText = "CAPTCHA Clearance token used to handle CloudFlare Anti-DDOS measures on shared-ip VPNs.")]
        public string CaptchaToken { get; set; }
                
        [FieldDefinition(3, Type = FieldType.Tag, SelectOptions = typeof(Language), Label = "Multi Languages", HelpText = "What languages are normally in a multi release on this indexer?", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(4, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(5, Type = FieldType.Tag, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", HelpLink = "https://github.com/Radarr/Radarr/wiki/Indexer-Flags#1-required-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
