using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.ImportLists.ImportExclusions;

namespace Radarr.Api.V3.ImportLists
{
    public class ImportListExclusionResource : ProviderResource<ImportListExclusionResource>
    {
        // public int Id { get; set; }
        public int TmdbId { get; set; }
        public string MovieTitle { get; set; }
        public int MovieYear { get; set; }
    }

    public static class ImportListExclusionResourceMapper
    {
        public static ImportListExclusionResource ToResource(this ImportListExclusion model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportListExclusionResource
            {
                Id = model.Id,
                TmdbId = model.TmdbId,
                MovieTitle = model.MovieTitle,
                MovieYear = model.MovieYear
            };
        }

        public static List<ImportListExclusionResource> ToResource(this IEnumerable<ImportListExclusion> exclusions)
        {
            return exclusions.Select(ToResource).ToList();
        }

        public static ImportListExclusion ToModel(this ImportListExclusionResource resource)
        {
            return new ImportListExclusion
            {
                Id = resource.Id,
                TmdbId = resource.TmdbId,
                MovieTitle = resource.MovieTitle,
                MovieYear = resource.MovieYear
            };
        }

        public static List<ImportListExclusion> ToModel(this IEnumerable<ImportListExclusionResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
