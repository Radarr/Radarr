using System.Collections.Generic;
using System.Linq;
using Radarr.Api.V2.Qualities;
using Radarr.Http.REST;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;
using NzbDrone.Core.CustomFormats;

namespace Radarr.Api.V2.Profiles.Quality
{
    public class QualityProfileResource : RestResource
    {
        public string Name { get; set; }
        public bool UpgradeAllowed { get; set; }
        public int Cutoff { get; set; }
        public string PreferredTags { get; set; }
        public List<QualityProfileQualityItemResource> Items { get; set; }
        public int FormatCutoff { get; set; }
        public List<ProfileFormatItemResource> FormatItems { get; set; }
        public Language Language { get; set; }
    }

    public class QualityProfileQualityItemResource : RestResource
    {
        public string Name { get; set; }
        public NzbDrone.Core.Qualities.Quality Quality { get; set; }
        public List<QualityProfileQualityItemResource> Items { get; set; }
        public bool Allowed { get; set; }

        public QualityProfileQualityItemResource()
        {
            Items = new List<QualityProfileQualityItemResource>();
        }
    }

    public class ProfileFormatItemResource : RestResource
    {
        public CustomFormat Format { get; set; }
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
                UpgradeAllowed = model.UpgradeAllowed,
                Cutoff = model.Cutoff,
                PreferredTags = model.PreferredTags != null ? string.Join(",", model.PreferredTags) : "",
                Items = model.Items.ConvertAll(ToResource),
                FormatCutoff = model.FormatCutoff,
                FormatItems = model.FormatItems.ConvertAll(ToResource),
                Language = model.Language
            };
        }

        public static QualityProfileQualityItemResource ToResource(this ProfileQualityItem model)
        {
            if (model == null) return null;

            return new QualityProfileQualityItemResource
            {
                Id = model.Id,
                Name = model.Name,
                Quality = model.Quality,
                Items = model.Items.ConvertAll(ToResource),
                Allowed = model.Allowed
            };
        }

        public static ProfileFormatItemResource ToResource(this ProfileFormatItem model)
        {
            return new ProfileFormatItemResource
            {
                Format = model.Format,
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
                UpgradeAllowed = resource.UpgradeAllowed,
                Cutoff = resource.Cutoff,
                PreferredTags = resource.PreferredTags.Split(',').ToList(),
                Items = resource.Items.ConvertAll(ToModel),
                FormatCutoff = resource.FormatCutoff,
                FormatItems = resource.FormatItems.ConvertAll(ToModel),
                Language = resource.Language
            };
        }

        public static ProfileQualityItem ToModel(this QualityProfileQualityItemResource resource)
        {
            if (resource == null) return null;

            return new ProfileQualityItem
            {
                Id = resource.Id,
                Name = resource.Name,
                Quality = resource.Quality != null ? (NzbDrone.Core.Qualities.Quality)resource.Quality.Id : null,
                Items = resource.Items.ConvertAll(ToModel),
                Allowed = resource.Allowed
            };
        }

        public static ProfileFormatItem ToModel(this ProfileFormatItemResource resource)
        {
            return new ProfileFormatItem
            {
                Format = resource.Format,
                Allowed = resource.Allowed
            };
        }

        public static List<QualityProfileResource> ToResource(this IEnumerable<Profile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
