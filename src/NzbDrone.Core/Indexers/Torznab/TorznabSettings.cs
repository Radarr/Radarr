using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Indexers.Newznab;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Indexers.Torznab
{
    public class TorznabSettingsValidator : AbstractValidator<TorznabSettings>
    {
        private static readonly string[] ApiKeyWhiteList =
        {
            "hd4free.xyz",
        };

        private static bool ShouldHaveApiKey(TorznabSettings settings)
        {
            if (settings.BaseUrl == null)
            {
                return false;
            }

            return ApiKeyWhiteList.Any(c => settings.BaseUrl.ToLowerInvariant().Contains(c));
        }

        private static readonly Regex AdditionalParametersRegex = new Regex(@"(&.+?\=.+?)+", RegexOptions.Compiled);

        public TorznabSettingsValidator()
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

    public class TorznabSettings : NewznabSettings, ITorrentIndexerSettings
    {
        private static readonly TorznabSettingsValidator Validator = new TorznabSettingsValidator();

        public TorznabSettings()
        {
            MinimumSeeders = IndexerDefaults.MINIMUM_SEEDERS;
        }

        [FieldDefinition(8, Type = FieldType.Number, Label = "Minimum Seeders", HelpText = "Minimum number of seeders required.", Advanced = true)]
        public int MinimumSeeders { get; set; }

        [FieldDefinition(9, Type = FieldType.Tag, SelectOptions = typeof(IndexerFlags), Label = "Required Flags", HelpText = "What indexer flags are required?", HelpLink = "https://github.com/Radarr/Radarr/wiki/Indexer-Flags#1-required-flags", Advanced = true)]
        public IEnumerable<int> RequiredFlags { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
