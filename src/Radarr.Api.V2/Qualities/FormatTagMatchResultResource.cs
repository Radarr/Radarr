using System.Collections.Generic;
using System.Linq;
using Radarr.Http.REST;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Qualities;
using System;

namespace Radarr.Api.V2.Qualities
{
    public class FormatTagMatchResultResource : RestResource
    {
        public CustomFormatResource CustomFormat { get; set; }
        public List<FormatTagGroupMatchesResource> GroupMatches { get; set; }
    }

    public class FormatTagGroupMatchesResource : RestResource
    {
        public string GroupName { get; set; }
        public IDictionary<string, bool> Matches { get; set; }
        public bool DidMatch { get; set; }
    }

    public class CustomFormatTestResource : RestResource
    {
        public List<FormatTagMatchResultResource> Matches { get; set; }
        public List<CustomFormatResource> MatchedFormats { get; set; }
    }

    public static class QualityTagMatchResultResourceMapper
    {
        public static FormatTagMatchResultResource ToResource(this FormatTagMatchResult model)
        {
            if (model == null) return null;

            return new FormatTagMatchResultResource
            {
                CustomFormat = model.CustomFormat.ToResource(),
                GroupMatches = model.GroupMatches.ToResource()
            };
        }

        public static List<FormatTagMatchResultResource> ToResource(this IList<FormatTagMatchResult> models)
        {
            return models.Select(ToResource).ToList();
        }

        public static FormatTagGroupMatchesResource ToResource(this FormatTagMatchesGroup model)
        {
            return new FormatTagGroupMatchesResource
            {
                GroupName = model.Type.ToString(),
                DidMatch = model.DidMatch,
                Matches = model.Matches.SelectDictionary(m => m.Key.Raw, m => m.Value)
            };
        }

        public static List<FormatTagGroupMatchesResource> ToResource(this IList<FormatTagMatchesGroup> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
