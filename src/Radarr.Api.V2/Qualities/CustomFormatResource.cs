using System.Collections.Generic;
using System.Linq;
using Radarr.Http.REST;
using NzbDrone.Core.CustomFormats;

namespace Radarr.Api.V2.Qualities
{
    public class CustomFormatResource : RestResource
    {
        public string Name { get; set; }
        public List<string> FormatTags { get; set; }
        public string Simplicity { get; set; }
    }

    public static class CustomFormatResourceMapper
    {
        public static CustomFormatResource ToResource(this CustomFormat model)
        {
            return new CustomFormatResource
            {
                Id = model.Id,
                Name = model.Name,
                FormatTags = model.FormatTags.Select(t => t.Raw.ToUpper()).ToList(),
            };
        }

        public static CustomFormat ToModel(this CustomFormatResource resource)
        {
            return new CustomFormat
            {
                Id = resource.Id,
                Name = resource.Name,
                FormatTags = resource.FormatTags.Select(s => new FormatTag(s)).ToList(),
            };
        }

        public static List<CustomFormatResource> ToResource(this IEnumerable<CustomFormat> models)
        {
            return models.Select(m => m.ToResource()).ToList();
        }
    }
}
