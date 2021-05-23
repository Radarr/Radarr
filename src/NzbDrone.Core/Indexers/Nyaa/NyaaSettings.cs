using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Nyaa
{
    public class NyaaSettingsValidator : AbstractValidator<NyaaSettings>
    {
        public NyaaSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.AdditionalParameters).Matches("(&[a-z]+=[a-z0-9_]+)*", RegexOptions.IgnoreCase);

            RuleFor(c => c.SeedCriteria).SetValidator(_ => new SeedCriteriaSettingsValidator());
        }
    }

    public class NyaaSettings : ITorrentIndexerSettings
    {
        private static readonly NyaaSettingsValidator Validator = new NyaaSettingsValidator();

        public NyaaSettings()
        {
            BaseUrl = "";
            AdditionalParameters = "&cats=1_0&filter=1";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
            MultiLanguages = new List<int>();
            RequiredFlags = new List<int>();
        }

        [FieldDefinition(0, Label = "Website URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Label = "Multi Languages", HelpText = "What languages are normally in a multi release on this indexer?", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(2, Label = "Additional Parameters", Advanced = true, HelpText = "Please note if you change the category you will have to add required/restricted rules about the subgroups to avoid foreign language releases.")]
        public string AdditionalParameters { get; set; }

        [FieldDefinition(3, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(4, Type = FieldType.TagSelect, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", HelpLink = "https://wiki.servarr.com/Definitions#Indexer_Flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        [FieldDefinition(5)]
        public SeedCriteriaSettings SeedCriteria { get; set; } = new SeedCriteriaSettings();

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
