using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.CustomFormats;
using Radarr.Http.REST;

namespace NzbDrone.Api.CustomFormats
{
    public class CustomFormatResource : RestResource
    {
        public string Name { get; set; }
        public List<ICustomFormatSpecification> Specifications { get; set; }
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
                Specifications = model.Specifications.ToList(),
            };
        }

        public static List<CustomFormatResource> ToResource(this IEnumerable<CustomFormat> models)
        {
            return models.Select(m => m.ToResource()).ToList();
        }

        public static CustomFormat ToModel(this CustomFormatResource resource)
        {
            return new CustomFormat
            {
                Id = resource.Id,
                Name = resource.Name,
                Specifications = resource.Specifications.ToList(),
            };
        }
    }
}
