using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.CustomFormats;
using Radarr.Http.REST;

namespace Radarr.Api.V3.CustomFormats
{
    public class CustomFormatResource : RestResource
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public override int Id { get; set; }
        public string Name { get; set; }
        public string FormatTags { get; set; }
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
                FormatTags = string.Join(",", model.FormatTags.Select(t => t.Raw.ToUpper()).ToList()),
            };
        }

        public static List<CustomFormatResource> ToResource(this IEnumerable<CustomFormat> models)
        {
            return models.Select(m => m.ToResource()).ToList();
        }

        public static CustomFormat ToModel(this CustomFormatResource resource)
        {
            if (resource.Id == 0 && resource.Name == "None")
            {
                return CustomFormat.None;
            }

            return new CustomFormat
            {
                Id = resource.Id,
                Name = resource.Name,
                FormatTags = resource.FormatTags.Split(',').Select(s => new FormatTag(s)).ToList()
            };
        }
    }
}
