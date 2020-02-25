using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles;
using Radarr.Api.V3.CustomFormats;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Profiles.Quality
{
    public class QualityProfileResource : RestResource
    {
        public string Name { get; set; }
        public bool UpgradeAllowed { get; set; }
        public int Cutoff { get; set; }
        public string PreferredTags { get; set; }
        public List<QualityProfileQualityItemResource> Items { get; set; }
        public int MinFormatScore { get; set; }
        public int CutoffFormatScore { get; set; }
        public List<ProfileFormatItemResource> FormatItems { get; set; }
        public Language Language { get; set; }
    }

    public class QualityProfileQualityItemResource : RestResource
    {
        public QualityProfileQualityItemResource()
        {
            Items = new List<QualityProfileQualityItemResource>();
        }

        public string Name { get; set; }
        public NzbDrone.Core.Qualities.Quality Quality { get; set; }
        public List<QualityProfileQualityItemResource> Items { get; set; }
        public bool Allowed { get; set; }
    }

    public class ProfileFormatItemResource : RestResource
    {
        public int Format { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
    }

    public static class ProfileResourceMapper
    {
        public static QualityProfileResource ToResource(this Profile model)
        {
            if (model == null)
            {
                return null;
            }

            return new QualityProfileResource
            {
                Id = model.Id,
                Name = model.Name,
                UpgradeAllowed = model.UpgradeAllowed,
                Cutoff = model.Cutoff,
                PreferredTags = model.PreferredTags != null ? string.Join(",", model.PreferredTags) : "",
                Items = model.Items.ConvertAll(ToResource),
                MinFormatScore = model.MinFormatScore,
                CutoffFormatScore = model.CutoffFormatScore,
                FormatItems = model.FormatItems.ConvertAll(ToResource),
                Language = model.Language
            };
        }

        public static QualityProfileQualityItemResource ToResource(this ProfileQualityItem model)
        {
            if (model == null)
            {
                return null;
            }

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
                Format = model.Format.Id,
                Name = model.Format.Name,
                Score = model.Score
            };
        }

        public static Profile ToModel(this QualityProfileResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Profile
            {
                Id = resource.Id,
                Name = resource.Name,
                UpgradeAllowed = resource.UpgradeAllowed,
                Cutoff = resource.Cutoff,
                PreferredTags = resource.PreferredTags.Split(',').ToList(),
                Items = resource.Items.ConvertAll(ToModel),
                MinFormatScore = resource.MinFormatScore,
                CutoffFormatScore = resource.CutoffFormatScore,
                FormatItems = resource.FormatItems.ConvertAll(ToModel),
                Language = resource.Language
            };
        }

        public static ProfileQualityItem ToModel(this QualityProfileQualityItemResource resource)
        {
            if (resource == null)
            {
                return null;
            }

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
                Format = new CustomFormat { Id = resource.Format },
                Score = resource.Score
            };
        }

        public static List<QualityProfileResource> ToResource(this IEnumerable<Profile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
