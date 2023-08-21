using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Movies.AlternativeTitles;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    public class AlternativeTitleResource : RestResource
    {
        // Todo: Sorters should be done completely on the client
        // Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        // Todo: We should get the entire Profile instead of ID and Name separately
        public SourceType SourceType { get; set; }
        public int MovieMetadataId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }

        // TODO: Add series statistics as a property of the series (instead of individual properties)
    }

    public static class AlternativeTitleResourceMapper
    {
        public static AlternativeTitleResource ToResource(this AlternativeTitle model)
        {
            if (model == null)
            {
                return null;
            }

            return new AlternativeTitleResource
            {
                Id = model.Id,
                SourceType = model.SourceType,
                MovieMetadataId = model.MovieMetadataId,
                Title = model.Title
            };
        }

        public static AlternativeTitle ToModel(this AlternativeTitleResource resource)
        {
            if (resource == null)
            {
                return null;
            }

            return new AlternativeTitle
            {
                Id = resource.Id,
                SourceType = resource.SourceType,
                MovieMetadataId = resource.MovieMetadataId,
                Title = resource.Title
            };
        }

        public static List<AlternativeTitleResource> ToResource(this IEnumerable<AlternativeTitle> movies)
        {
            return movies.Select(ToResource).ToList();
        }
    }
}
