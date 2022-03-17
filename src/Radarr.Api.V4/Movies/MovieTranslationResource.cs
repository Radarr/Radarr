using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies.Translations;
using Radarr.Http.REST;

namespace Radarr.Api.V4.Movies
{
    public class MovieTranslationResource : RestResource
    {
        public int MovieMetadataId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public Language Language { get; set; }
    }

    public static class MovieTranslationResourceMapper
    {
        public static MovieTranslationResource ToResource(this MovieTranslation model)
        {
            if (model == null)
            {
                return null;
            }

            return new MovieTranslationResource
            {
                Id = model.Id,
                MovieMetadataId = model.MovieMetadataId,
                Title = model.Title,
                Language = model.Language
            };
        }

        public static MovieTranslation ToModel(this MovieTranslationResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new MovieTranslation
            {
                Id = resource.Id,
                MovieMetadataId = resource.MovieMetadataId,
                Title = resource.Title,
                Language = resource.Language
            };
        }

        public static List<MovieTranslationResource> ToResource(this IEnumerable<MovieTranslation> movies)
        {
            return movies.Select(ToResource).ToList();
        }
    }
}
