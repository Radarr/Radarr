using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Parser;

namespace NzbDrone.Api.Movies
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
        public int MovieId { get; set; }
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
    }
}
