using System.Collections.Generic;
using System.Linq;
using Radarr.Api.V2.Qualities;
using Radarr.Http.REST;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace Radarr.Api.V2.Profiles.Quality
{
    public class QualityProfileResource : RestResource
    {
        public string Name { get; set; }
        public NzbDrone.Core.Qualities.Quality Cutoff { get; set; }
        public string PreferredTags { get; set; }
        public List<ProfileQualityItemResource> Items { get; set; }
        public CustomFormatResource FormatCutoff { get; set; }
        public List<ProfileFormatItemResource> FormatItems { get; set; }
        public Language Language { get; set; }
    }

    public class ProfileQualityItemResource : RestResource
    {
        public NzbDrone.Core.Qualities.Quality Quality { get; set; }
        public bool Allowed { get; set; }
    }

    public class ProfileFormatItemResource : RestResource
    {
        public CustomFormatResource Format { get; set; }
        public bool Allowed { get; set; }
    }

    public static class ProfileResourceMapper
    {
        public static QualityProfileResource ToResource(this Profile model)
        {
            if (model == null) return null;

            return new QualityProfileResource
            {
                Id = model.Id,

                Name = model.Name,
                Cutoff = model.Cutoff,
                PreferredTags = model.PreferredTags != null ? string.Join(",", model.PreferredTags) : "",
                Items = model.Items.ConvertAll(ToResource),
                FormatCutoff = model.FormatCutoff.ToResource(),
                FormatItems = model.FormatItems.ConvertAll(ToResource),
                Language = model.Language
            };
        }

        public static ProfileQualityItemResource ToResource(this ProfileQualityItem model)
        {
            if (model == null) return null;

            return new ProfileQualityItemResource
            {
                Quality = model.Quality,
                Allowed = model.Allowed
            };
        }

        public static ProfileFormatItemResource ToResource(this ProfileFormatItem model)
        {
            return new ProfileFormatItemResource
            {
                Format = model.Format.ToResource(),
                Allowed = model.Allowed
            };
        }

        public static Profile ToModel(this QualityProfileResource resource)
        {
            if (resource == null) return null;

            return new Profile
            {
                Id = resource.Id,

                Name = resource.Name,
                Cutoff = (NzbDrone.Core.Qualities.Quality)resource.Cutoff.Id,
                PreferredTags = resource.PreferredTags.Split(',').ToList(),
                Items = resource.Items.ConvertAll(ToModel),
                FormatCutoff = resource.FormatCutoff.ToModel(),
                FormatItems = resource.FormatItems.ConvertAll(ToModel),
                Language = resource.Language
            };
        }

        public static ProfileQualityItem ToModel(this ProfileQualityItemResource resource)
        {
            if (resource == null) return null;

            return new ProfileQualityItem
            {
                Quality = (NzbDrone.Core.Qualities.Quality)resource.Quality.Id,
                Allowed = resource.Allowed
            };
        }

        public static ProfileFormatItem ToModel(this ProfileFormatItemResource resource)
        {
            return new ProfileFormatItem
            {
                Format = resource.Format.ToModel(),
                Allowed = resource.Allowed
            };
        }

        public static List<QualityProfileResource> ToResource(this IEnumerable<Profile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
