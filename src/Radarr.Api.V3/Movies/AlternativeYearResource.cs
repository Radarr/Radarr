using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    public class AlternativeYearResource : RestResource
    {
        public AlternativeYearResource()
        {
        }

        //Todo: Sorters should be done completely on the client
        //Todo: Is there an easy way to keep IgnoreArticlesWhenSorting in sync between, Series, History, Missing?
        //Todo: We should get the entire Profile instead of ID and Name separately
        public int MovieId { get; set; }
        public int Year { get; set; }

        //TODO: Add series statistics as a property of the series (instead of individual properties)
    }

    /*public static class AlternativeYearResourceMapper
    {
        /*public static AlternativeYearResource ToResource(this AlternativeTitle model)
        {
            if (model == null) return null;

            AlternativeTitleResource resource = null;

            return new AlternativeTitleResource
            {
                Id = model.Id,
                SourceType = model.SourceType,
                MovieId = model.MovieId,
                Title = model.Title,
                SourceId = model.SourceId,
                Votes = model.Votes,
                VoteCount = model.VoteCount,
                Language = model.Language
            };
        }

        public static AlternativeTitle ToModel(this AlternativeTitleResource resource)
        {
            if (resource == null) return null;

            return new AlternativeTitle
            {
                Id = resource.Id,
                SourceType = resource.SourceType,
                MovieId = resource.MovieId,
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
    }*/
}
