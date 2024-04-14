using System;
using System.Collections.Generic;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Radarr
{
    public class RadarrSettingsValidator : AbstractValidator<RadarrSettings>
    {
        public RadarrSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class RadarrSettings : ImportListSettingsBase<RadarrSettings>
    {
        private static readonly RadarrSettingsValidator Validator = new ();

        public RadarrSettings()
        {
            ApiKey = "";
            ProfileIds = Array.Empty<int>();
            TagIds = Array.Empty<int>();
            RootFolderPaths = Array.Empty<string>();
        }

        [FieldDefinition(0, Label = "Full URL", HelpText = "URL, including port, of the Radarr instance to import from (Radarr 3.0 or higher)")]
        public string BaseUrl { get; set; } = string.Empty;

        [FieldDefinition(1, Label = "API Key", Privacy = PrivacyLevel.ApiKey, HelpText = "Apikey of the Radarr instance to import from (Radarr 3.0 or higher)")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, SelectOptionsProviderAction = "getProfiles", Label = "Profiles", HelpText = "Profiles from the source instance to import from")]
        public IEnumerable<int> ProfileIds { get; set; }

        [FieldDefinition(3, Type = FieldType.Select, SelectOptionsProviderAction = "getTags", Label = "Tags", HelpText = "Tags from the source instance to import from")]
        public IEnumerable<int> TagIds { get; set; }

        [FieldDefinition(4, Type = FieldType.Select, SelectOptionsProviderAction = "getRootFolders", Label = "Root Folders", HelpText = "Root Folders from the source instance to import from")]
        public IEnumerable<string> RootFolderPaths { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
