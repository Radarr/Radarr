﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabSettingsValidator : AbstractValidator<NewznabSettings>
    {
        private static readonly string[] ApiKeyWhiteList =
        {
            "nzbs.org",
            "nzb.su",
            "dognzb.cr",
            "nzbplanet.net",
            "nzbid.org",
            "nzbndx.com",
            "nzbindex.in"
        };

        private static bool ShouldHaveApiKey(NewznabSettings settings)
        {
            if (settings.BaseUrl == null)
            {
                return false;
            }

            return ApiKeyWhiteList.Any(c => settings.BaseUrl.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new Regex(@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public NewznabSettingsValidator()
        {
            Custom(newznab =>
            {
                if (newznab.Categories.Empty() && newznab.AnimeCategories.Empty())
                {
                    return new ValidationFailure("", "Either 'Categories' or 'Anime Categories' must be provided");
                }

                return null;
            });

            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty().When(ShouldHaveApiKey);
            RuleFor(c => c.AdditionalParameters).Matches(AdditionalParametersRegex)
                                                .When(c => !c.AdditionalParameters.IsNullOrWhiteSpace());
        }
    }

    public class NewznabSettings : IIndexerSettings
    {
        private static readonly NewznabSettingsValidator Validator = new NewznabSettingsValidator();

        public NewznabSettings()
        {
            Categories = new[] { 2000, 2010, 2020, 2030, 2035, 2040, 2045, 2050, 2060 };
            AnimeCategories = Enumerable.Empty<int>();
        }

        [FieldDefinition(0, Label = "URL")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key")]
        public string ApiKey { get; set; }

        [FieldDefinition(2, Label = "Categories", HelpText = "Comma Separated list, leave blank to disable all categories", Advanced = true)]
        public IEnumerable<int> Categories { get; set; }

        [FieldDefinition(3, Label = "Anime Categories", HelpText = "Comma Separated list, leave blank to disable anime", Advanced = true)]
        public IEnumerable<int> AnimeCategories { get; set; }

        [FieldDefinition(4, Label = "Additional Parameters", HelpText = "Additional Newznab parameters", Advanced = true)]
        public string AdditionalParameters { get; set; }

        [FieldDefinition(5, Label = "Remove year from search string",
            HelpText = "Should Radarr remove the year after the title when searching this indexer?", Advanced = true, Type = FieldType.Checkbox)]
        public bool RemoveYear { get; set; }

        [FieldDefinition(6, Label = "Search by Title",
            HelpText = "By default, Radarr will try to search by IMDB ID if your indexer supports that. However, some indexers are not very good at tagging their releases correctly, so you can force Radarr to search that indexer by title instead.",
            Advanced = true, Type = FieldType.Checkbox)]
        public bool SearchByTitle { get; set; }
        // Field 7 is used by TorznabSettings MinimumSeeders
        // If you need to add another field here, update TorznabSettings as well and this comment

        public virtual NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}