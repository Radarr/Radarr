using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.NetImport.ImportExclusions;

namespace Radarr.Api.V3.NetImport
{
    public class ImportExclusionsResource : ProviderResource
    {
        //public int Id { get; set; }
        public int TmdbId { get; set; }
        public string MovieTitle { get; set; }
        public int MovieYear { get; set; }
    }

    public static class ImportExclusionsResourceMapper
    {
        public static ImportExclusionsResource ToResource(this ImportExclusion model)
        {
            if (model == null)
            {
                return null;
            }

            return new ImportExclusionsResource
            {
                Id = model.Id,
                TmdbId = model.TmdbId,
                MovieTitle = model.MovieTitle,
                MovieYear = model.MovieYear
            };
        }

        public static List<ImportExclusionsResource> ToResource(this IEnumerable<ImportExclusion> exclusions)
        {
            return exclusions.Select(ToResource).ToList();
        }

        public static ImportExclusion ToModel(this ImportExclusionsResource resource)
        {
            return new ImportExclusion
            {
                TmdbId = resource.TmdbId,
                MovieTitle = resource.MovieTitle,
                MovieYear = resource.MovieYear
            };
        }

        public static List<ImportExclusion> ToModel(this IEnumerable<ImportExclusionsResource> resources)
        {
            return resources.Select(ToModel).ToList();
        }
    }
}
