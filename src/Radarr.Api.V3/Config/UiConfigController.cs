using System;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using Radarr.Http;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Config
{
    [V3ApiController("config/ui")]
    public class UiConfigController : ConfigController<UiConfigResource>
    {
        private static readonly Regex HexColorRegex = new Regex("^#[0-9a-fA-F]{6}$", RegexOptions.Compiled);

        private readonly IConfigFileProvider _configFileProvider;

        public UiConfigController(IConfigFileProvider configFileProvider, IConfigService configService)
            : base(configService)
        {
            _configFileProvider = configFileProvider;

            SharedValidator.RuleFor(c => c.MovieInfoLanguage)
                .GreaterThanOrEqualTo(1)
                .WithMessage("The Movie Info Language value cannot be less than 1");

            SharedValidator.RuleFor(c => c.MovieInfoLanguage)
                .Must(value => Language.All.Any(o => o.Id == value))
                .WithMessage("Invalid Movie Info Language ID");

            SharedValidator.RuleFor(c => c.UILanguage)
                .GreaterThanOrEqualTo(1)
                .WithMessage("The UI Language value cannot be less than 1");

            SharedValidator.RuleFor(c => c.UILanguage)
                .Must(value => Language.All.Any(o => o.Id == value))
                .WithMessage("Invalid UI Language ID");

            SharedValidator.RuleFor(c => c.PosterReplacementBackgroundColor)
                .Matches(HexColorRegex)
                .When(c => c.PosterReplacementBackgroundColor != null)
                .WithMessage("Must be a hex color, e.g. #1c1c1c");

            SharedValidator.RuleFor(c => c.PosterReplacementTextColor)
                .Matches(HexColorRegex)
                .When(c => c.PosterReplacementTextColor != null)
                .WithMessage("Must be a hex color, e.g. #ffffff");

            SharedValidator.RuleForEach(c => c.PosterReplacementGenres)
                .Must(genre => genre == null || !genre.Contains(','))
                .WithMessage("Genres cannot contain commas");
        }

        [RestPutById]
        public override ActionResult<UiConfigResource> SaveConfig([FromBody] UiConfigResource resource)
        {
            var dictionary = resource.GetType()
                                     .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                     .ToDictionary(prop => prop.Name, prop => prop.GetValue(resource, null));

            // List settings are persisted as comma separated strings
            dictionary[nameof(UiConfigResource.PosterReplacementGenres)] = resource.PosterReplacementGenres?
                .Where(g => g.IsNotNullOrWhiteSpace())
                .Select(g => g.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Join(",");

            dictionary[nameof(UiConfigResource.PosterReplacementTags)] = resource.PosterReplacementTags?
                .Distinct()
                .Select(t => t.ToString())
                .Join(",");

            _configFileProvider.SaveConfigDictionary(dictionary);
            _configService.SaveConfigDictionary(dictionary);

            return Accepted(resource.Id);
        }

        protected override UiConfigResource ToResource(IConfigService model)
        {
            return UiConfigResourceMapper.ToResource(_configFileProvider, model);
        }
    }
}
