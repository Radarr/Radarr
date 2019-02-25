using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.TorrentPotato
{
    public class TorrentPotatoSettingsValidator : AbstractValidator<TorrentPotatoSettings>
    {
        public TorrentPotatoSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
        }
    }

    public class TorrentPotatoSettings : ITorrentIndexerSettings
    {
        private static readonly TorrentPotatoSettingsValidator Validator = new TorrentPotatoSettingsValidator();

        public TorrentPotatoSettings()
        {
            BaseUrl = "http://127.0.0.1";
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
        }

        [FieldDefinition(0, Label = "API URL", HelpText = "URL to TorrentPotato api.")]
        public string BaseUrl { get; set; }
        
        [FieldDefinition(1, Label = "Username", HelpText = "The username you use at your indexer.")]
        public string User { get; set; }

        [FieldDefinition(2, Label = "Passkey", HelpText = "The password you use at your Indexer.")]
        public string Passkey { get; set; }
                
        [FieldDefinition(3, Type = FieldType.Tag, SelectOptions = typeof(Language), Label = "Multi Languages", HelpText = "What languages are normally in a multi release on this indexer?", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(4, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }
        
        [FieldDefinition(5, Type = FieldType.Tag, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
