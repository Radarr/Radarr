using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Profiles.Metadata;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Profiles.Metadata
{
    public class MetadataProfileResource : RestResource
    {
        public string Name { get; set; }
        public double MinPopularity { get; set; }
        public bool SkipMissingDate { get; set; }
        public bool SkipMissingIsbn { get; set; }
        public bool SkipPartsAndSets { get; set; }
        public bool SkipSeriesSecondary { get; set; }
        public string AllowedLanguages { get; set; }
    }

    public static class MetadataProfileResourceMapper
    {
        public static MetadataProfileResource ToResource(this MetadataProfile model)
        {
            if (model == null)
            {
                return null;
            }

            return new MetadataProfileResource
            {
                Id = model.Id,
                Name = model.Name,
                MinPopularity = model.MinPopularity,
                SkipMissingDate = model.SkipMissingDate,
                SkipMissingIsbn = model.SkipMissingIsbn,
                SkipPartsAndSets = model.SkipPartsAndSets,
                SkipSeriesSecondary = model.SkipSeriesSecondary,
                AllowedLanguages = model.AllowedLanguages
            };
        }

        public static MetadataProfile ToModel(this MetadataProfileResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new MetadataProfile
            {
                Id = resource.Id,
                Name = resource.Name,
                MinPopularity = resource.MinPopularity,
                SkipMissingDate = resource.SkipMissingDate,
                SkipMissingIsbn = resource.SkipMissingIsbn,
                SkipPartsAndSets = resource.SkipPartsAndSets,
                SkipSeriesSecondary = resource.SkipSeriesSecondary,
                AllowedLanguages = resource.AllowedLanguages
            };
        }

        public static List<MetadataProfileResource> ToResource(this IEnumerable<MetadataProfile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
