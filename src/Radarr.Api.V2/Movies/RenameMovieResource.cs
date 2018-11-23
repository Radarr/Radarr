using NzbDrone.Core.MediaFiles;
using Radarr.Http.REST;
using System.Collections.Generic;
using System.Linq;

namespace Radarr.Api.V2.Movies
{
    public class RenameMovieResource : RestResource
    {
        public int MovieId { get; set; }
        public int MovieFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }

    public static class RenameMovieResourceMapper
    {
        public static RenameMovieResource ToResource(this RenameMovieFilePreview model)
        {
            if (model == null) return null;

            return new RenameMovieResource
            {
                MovieId = model.MovieId,
                MovieFileId = model.MovieFileId,
                ExistingPath = model.ExistingPath,
                NewPath = model.NewPath
            };
        }

        public static List<RenameMovieResource> ToResource(this IEnumerable<RenameMovieFilePreview> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
