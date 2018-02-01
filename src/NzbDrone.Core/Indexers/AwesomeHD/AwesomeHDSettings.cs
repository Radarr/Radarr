using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.AwesomeHD
{
    public class AwesomeHDSettingsValidator : AbstractValidator<AwesomeHDSettings>
    {
        public AwesomeHDSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.Passkey).NotEmpty();
        }
    }

    public class AwesomeHDSettings : ITorrentIndexerSettings
    {
        private static readonly AwesomeHDSettingsValidator Validator = new AwesomeHDSettingsValidator();

        public AwesomeHDSettings()
        {
            BaseUrl = "https://awesome-hd.me";
            MinimumSeeders = 0;
        }

        [FieldDefinition(0, Label = "API URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since you Passkey will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "Passkey")]
        public string Passkey { get; set; }

        [FieldDefinition(2, Type = FieldType.Textbox, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }
        
        [FieldDefinition(3, Type = FieldType.Tag, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
