using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;
using System.Text.RegularExpressions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornSettingsValidator : AbstractValidator<PassThePopcornSettings>
    {
        public PassThePopcornSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.Username).Empty();
            RuleFor(c => c.Password).Empty();
            RuleFor(c => c.Passkey).Empty();
            RuleFor(c => c.APIUser).NotEmpty();
            RuleFor(c => c.APIKey).NotEmpty();
        }
    }

    public class PassThePopcornSettings : ITorrentIndexerSettings
    {
        private static readonly PassThePopcornSettingsValidator Validator = new PassThePopcornSettingsValidator();

        public PassThePopcornSettings()
        {
            BaseUrl = "https://passthepopcorn.me";
            MinimumSeeders = 0;
        }

        [FieldDefinition(0, Label = "URL", Advanced = true, HelpText = "Do not change this unless you know what you're doing. Since your cookie will be sent to that host.")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "APIUser", HelpText = "These settings are found in your PassThePopcorn security settings (Edit Profile > Security).")]
        public string APIUser { get; set; }
        
        [FieldDefinition(2, Label = "APIKey", Type = FieldType.Password)]
        public string APIKey { get; set; }

        [FieldDefinition(3, Label = "DEPRECATED: User", HelpText = "Please use APIKey & APIUser instead. PTP Username")]
        public string Username { get; set; }

        [FieldDefinition(4, Label = "DEPRECATED: Pass", Type = FieldType.Password, HelpText = "Please use APIKey & APIUser instead. PTP Password")]
        public string Password { get; set; }

        [FieldDefinition(5, Label = "DEPRECATED: Passkey",  HelpText = "Please use APIKey & APIUser instead. PTP Passkey")]
        public string Passkey { get; set; }
              
        [FieldDefinition(6, Type = FieldType.Tag, SelectOptions = typeof(Language), Label = "Multi Languages", HelpText = "What languages are normally in a multi release on this indexer?", Advanced = true)]
        public IEnumerable<int> MultiLanguages { get; set; }

        [FieldDefinition(7, Type = FieldType.Textbox, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(8, Type = FieldType.Tag, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", HelpLink = "https://github.com/Radarr/Radarr/wiki/Indexer-Flags#1-required-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
