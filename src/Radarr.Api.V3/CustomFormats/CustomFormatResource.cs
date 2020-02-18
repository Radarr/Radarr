using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NzbDrone.Core.CustomFormats;
using Radarr.Http.ClientSchema;
using Radarr.Http.REST;

namespace Radarr.Api.V3.CustomFormats
{
    public class CustomFormatResource : RestResource
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public override int Id { get; set; }
        public string Name { get; set; }
        public List<CustomFormatSpecificationSchema> Specifications { get; set; }
    }

    public static class CustomFormatResourceMapper
    {
        public static CustomFormatResource ToResource(this CustomFormat model)
        {
            return new CustomFormatResource
            {
                Id = model.Id,
                Name = model.Name,
                Specifications = model.Specifications.Select(x => x.ToSchema()).ToList(),
            };
        }

        public static List<CustomFormatResource> ToResource(this IEnumerable<CustomFormat> models)
        {
            return models.Select(m => m.ToResource()).ToList();
        }

        public static CustomFormat ToModel(this CustomFormatResource resource, List<ICustomFormatSpecification> specifications)
        {
            if (resource.Id == 0 && resource.Name == "None")
            {
                return CustomFormat.None;
            }

            return new CustomFormat
            {
                Id = resource.Id,
                Name = resource.Name,
                Specifications = resource.Specifications.Select(x => MapSpecification(x, specifications)).ToList()
            };
        }

        private static ICustomFormatSpecification MapSpecification(CustomFormatSpecificationSchema resource, List<ICustomFormatSpecification> specifications)
        {
            var type = specifications.SingleOrDefault(x => x.GetType().Name == resource.Implementation).GetType();
            var spec = (ICustomFormatSpecification)SchemaBuilder.ReadFromSchema(resource.Fields, type);
            spec.Name = resource.Name;
            spec.Negate = resource.Negate;
            spec.Required = resource.Required;
            return spec;
        }
    }
}
