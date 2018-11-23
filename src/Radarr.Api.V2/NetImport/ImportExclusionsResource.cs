using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.NetImport;
using NzbDrone.Core.Movies;

namespace Radarr.Api.V2.NetImport
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
        public static ImportExclusionsResource ToResource(this NzbDrone.Core.NetImport.ImportExclusions.ImportExclusion model)
        {
            if (model == null) return null;

            return new ImportExclusionsResource
            {
                Id = model.Id,
                TmdbId = model.TmdbId,
                MovieTitle = model.MovieTitle,
                MovieYear = model.MovieYear
            };
        }

        public static List<ImportExclusionsResource> ToResource(this IEnumerable<NzbDrone.Core.NetImport.ImportExclusions.ImportExclusion> exclusions)
        {
            return exclusions.Select(ToResource).ToList();
        }

        public static NzbDrone.Core.NetImport.ImportExclusions.ImportExclusion ToModel(this ImportExclusionsResource resource)
        {
            return new NzbDrone.Core.NetImport.ImportExclusions.ImportExclusion
            {
                TmdbId = resource.TmdbId,
                MovieTitle = resource.MovieTitle,
                MovieYear = resource.MovieYear
            };
        }
    }
}
