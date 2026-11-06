using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Movies.Versions;
using Radarr.Http.REST;

namespace Radarr.Api.V3.MovieVersions
{
    public class MovieVersionResource : RestResource
    {
        public int MovieId { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public int MovieFileId { get; set; }
        public bool HasFile { get; set; }
        public bool IsPrimary { get; set; }

        // Only used when adding a version: empty to share the movie's folder
        public string RootFolderPath { get; set; }
    }

    public static class MovieVersionResourceMapper
    {
        public static MovieVersionResource ToResource(this MovieVersion model, int primaryVersionId)
        {
            if (model == null)
            {
                return null;
            }

            return new MovieVersionResource
            {
                Id = model.Id,
                MovieId = model.MovieId,
                QualityProfileId = model.QualityProfileId,
                Path = model.Path,
                MovieFileId = model.MovieFileId,
                HasFile = model.HasFile,
                IsPrimary = model.Id == primaryVersionId
            };
        }

        public static List<MovieVersionResource> ToResource(this IEnumerable<MovieVersion> models)
        {
            var list = models.OrderBy(v => v.Id).ToList();
            var primaryId = list.FirstOrDefault()?.Id ?? 0;

            return list.Select(v => v.ToResource(primaryId)).ToList();
        }
    }
}
