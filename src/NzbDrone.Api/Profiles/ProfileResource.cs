using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.Qualities;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using Radarr.Http.REST;

namespace NzbDrone.Api.Profiles
{
    public class ProfileResource : RestResource
    {
        public string Name { get; set; }
        public Quality Cutoff { get; set; }
        public string PreferredTags { get; set; }
        public List<ProfileQualityItemResource> Items { get; set; }
        public CustomFormatResource FormatCutoff { get; set; }
        public List<ProfileFormatItemResource> FormatItems { get; set; }
        public Language Language { get; set; }
    }

    public class ProfileQualityItemResource : RestResource
    {
        public Quality Quality { get; set; }
        public bool Allowed { get; set; }
    }

    public class ProfileFormatItemResource : RestResource
    {
        public CustomFormatResource Format { get; set; }
        public bool Allowed { get; set; }
    }

    public static class ProfileResourceMapper
    {
        public static ProfileResource ToResource(this Profile model)
        {
            if (model == null)
            {
                return null;
            }

            var cutoffItem = model.Items.First(q =>
            {
                if (q.Id == model.Cutoff)
                {
                    return true;
                }

                if (q.Quality == null)
                {
                    return false;
                }

                return q.Quality.Id == model.Cutoff;
            });

            var cutoff = cutoffItem.Items == null || cutoffItem.Items.Empty()
                ? cutoffItem.Quality
                : cutoffItem.Items.First().Quality;

            var formatCutoffItem = model.FormatItems.First(q =>
            {
                if (q.Id == model.FormatCutoff)
                {
                    return true;
                }

                if (q.Format == null)
                {
                    return false;
                }

                return q.Format.Id == model.FormatCutoff;
            });

            var formatCutoff = formatCutoffItem.Format;

            return new ProfileResource
            {
                Id = model.Id,

                Name = model.Name,
                PreferredTags = model.PreferredTags != null ? string.Join(",", model.PreferredTags) : "",
                Cutoff = cutoff,

                // Flatten groups so things don't explode
                Items = model.Items.SelectMany(i =>
                {
                    if (i == null)
                    {
                        return null;
                    }

                    if (i.Items.Any())
                    {
                        return i.Items.ConvertAll(ToResource);
                    }

                    return new List<ProfileQualityItemResource> { ToResource(i) };
                }).ToList(),
                FormatCutoff = formatCutoff.ToResource(),
                FormatItems = model.FormatItems.ConvertAll(ToResource),
                Language = model.Language
            };
        }

        public static ProfileQualityItemResource ToResource(this ProfileQualityItem model)
        {
            if (model == null)
            {
                return null;
            }

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

        public static Profile ToModel(this ProfileResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Profile
            {
                Id = resource.Id,

                Name = resource.Name,
                Cutoff = resource.Cutoff.Id,
                PreferredTags = resource.PreferredTags.Split(',').ToList(),
                Items = resource.Items.ConvertAll(ToModel),
                FormatCutoff = resource.FormatCutoff.ToModel().Id,
                FormatItems = resource.FormatItems.ConvertAll(ToModel),
                Language = resource.Language
            };
        }

        public static ProfileQualityItem ToModel(this ProfileQualityItemResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new ProfileQualityItem
            {
                Quality = (Quality)resource.Quality.Id,
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

        public static List<ProfileResource> ToResource(this IEnumerable<Profile> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
