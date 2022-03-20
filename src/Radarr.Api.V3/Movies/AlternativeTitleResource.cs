using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies.AlternativeTitles;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    public class AlternativeTitleResource : RestResource
    {
        public AlternativeTitleResource()
        {
        }

        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately
        public SourceType SourceType { get; set; }
        public int MovieMetadataId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public int SourceId { get; set; }
        public int Votes { get; set; }
        public int VoteCount { get; set; }
        public Language Language { get; set; }

        //TODO: Add series statistics as a property of the series (instead of individual properties)
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
                Title = model.Title,
                SourceId = model.SourceId,
                Votes = model.Votes,
                VoteCount = model.VoteCount,
                Language = model.Language
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
                Title = resource.Title,
                SourceId = resource.SourceId,
                Votes = resource.Votes,
                VoteCount = resource.VoteCount,
                Language = resource.Language
            };
        }

        public static List<AlternativeTitleResource> ToResource(this IEnumerable<AlternativeTitle> movies)
        {
            return movies.Select(ToResource).ToList();
        }
    }
}
