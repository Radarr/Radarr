using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using System.Linq;
using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Movie
{
    public class MovieDiscoverModule : NzbDroneRestModule<MovieResource>
    {
        private readonly IDiscoverNewMovies _searchProxy;

        public MovieDiscoverModule(IDiscoverNewMovies searchProxy)
            : base("/movies/discover")
        {
            _searchProxy = searchProxy;
            Get["/{action?recommendations}"] = x => Search(x.action);
        }

        private Response Search(string action)
        {
            var imdbResults = _searchProxy.DiscoverNewMovies(action);
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