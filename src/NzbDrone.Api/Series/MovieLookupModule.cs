using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using System.Linq;

namespace NzbDrone.Api.Movie
{
    public class MovieLookupModule : NzbDroneRestModule<MovieResource>
    {
        private readonly ISearchForNewMovie _searchProxy;

        public MovieLookupModule(ISearchForNewMovie searchProxy)
            : base("/movies/lookup")
        {
            _searchProxy = searchProxy;
            Get["/"] = x => Search();
        }


        private Response Search()
        {
            var imdbResults = _searchProxy.SearchForNewMovie((string)Request.Query.term);
            return MapToResource(imdbResults).AsResponse();
        }


        private static IEnumerable<MovieResource> MapToResource(IEnumerable<Core.Tv.Movie> movies)
        {
            foreach (var currentSeries in movies)
            {
                var resource = currentSeries.ToResource();
                var poster = currentSeries.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                yield return resource;
            }
        }
    }
}