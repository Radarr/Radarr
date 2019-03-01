using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.ImportLists.Exclusions;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.ImportLists
{
    public class ImportListExclusionResource : RestResource
    {
        public string ForeignId { get; set; }
        public string ArtistName { get; set; }
    }

    public static class ImportListExclusionResourceMapper
    {
        public static ImportListExclusionResource ToResource(this ImportListExclusion model)
        {
            if (model == null) return null;

            return new ImportListExclusionResource
            {
                Id = model.Id,
                ForeignId = model.ForeignId,
                ArtistName = model.Name,
            };
        }

        public static ImportListExclusion ToModel(this ImportListExclusionResource resource)
        {
            if (resource == null) return null;

            return new ImportListExclusion
            {
                Id = resource.Id,
                ForeignId = resource.ForeignId,
                Name = resource.ArtistName
            };
        }

        public static List<ImportListExclusionResource> ToResource(this IEnumerable<ImportListExclusion> filters)
        {
            return filters.Select(ToResource).ToList();
        }
    }
}
