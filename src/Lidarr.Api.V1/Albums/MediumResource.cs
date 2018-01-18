using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Music;

namespace Lidarr.Api.V1.Albums
{
    public class MediumResource
    {
        public int MediumNumber { get; set; }
        public string MediumName { get; set; }
        public string MediumFormat { get; set; }
    }

    public static class MediumResourceMapper
    {
        public static MediumResource ToResource(this Medium model)
        {
            if (model == null)
            {
                return null;
            }

            return new MediumResource
            {
                MediumNumber = model.Number,
                MediumName = model.Name,
                MediumFormat = model.Format
            };
        }

        public static Medium ToModel(this MediumResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new Medium
            {
                Number = resource.MediumNumber,
                Name = resource.MediumName,
                Format = resource.MediumFormat
            };
        }

        public static List<MediumResource> ToResource(this IEnumerable<Medium> models)
        {
            return models.Select(ToResource).ToList();
        }

        public static List<Medium> ToModel(this IEnumerable<MediumResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
