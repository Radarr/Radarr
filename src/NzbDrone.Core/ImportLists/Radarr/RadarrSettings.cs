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

        [FieldDefinition(0, Label = "ImportListsRadarrSettingsFullUrl", HelpText = "ImportListsRadarrSettingsFullUrlHelpText")]
        public string BaseUrl { get; set; } = string.Empty;

        [FieldDefinition(1, Label = "ApiKey", Privacy = PrivacyLevel.ApiKey, HelpText = "ImportListsRadarrSettingsApiKeyHelpText")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Type = FieldType.Select, SelectOptionsProviderAction = "getProfiles", Label = "QualityProfiles", HelpText = "ImportListsRadarrSettingsQualityProfilesHelpText")]
        public IEnumerable<int> ProfileIds { get; set; }

        [FieldDefinition(3, Type = FieldType.Select, SelectOptionsProviderAction = "getTags", Label = "Tags", HelpText = "ImportListsRadarrSettingsTagsHelpText")]
        public IEnumerable<int> TagIds { get; set; }

        [FieldDefinition(4, Type = FieldType.Select, SelectOptionsProviderAction = "getRootFolders", Label = "RootFolders", HelpText = "ImportListsRadarrSettingsRootFoldersHelpText")]
        public IEnumerable<string> RootFolderPaths { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
