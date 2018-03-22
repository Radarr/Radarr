using NzbDrone.Api.REST;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Api.Movies
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
        public static RenameMovieResource ToResource(this Core.MediaFiles.RenameMovieFilePreview model)
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

        public static List<RenameMovieResource> ToResource(this IEnumerable<Core.MediaFiles.RenameMovieFilePreview> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
