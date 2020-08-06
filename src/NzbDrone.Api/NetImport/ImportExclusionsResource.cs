using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Api.ImportList
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
        public static ImportExclusionsResource ToResource(this Core.ImportLists.ImportExclusions.ImportExclusion model)
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

        public static List<ImportExclusionsResource> ToResource(this IEnumerable<Core.ImportLists.ImportExclusions.ImportExclusion> exclusions)
        {
            return exclusions.Select(ToResource).ToList();
        }

        public static Core.ImportLists.ImportExclusions.ImportExclusion ToModel(this ImportExclusionsResource resource)
        {
            return new Core.ImportLists.ImportExclusions.ImportExclusion
            {
                TmdbId = resource.TmdbId,
                MovieTitle = resource.MovieTitle,
                MovieYear = resource.MovieYear
            };
        }
    }
}
