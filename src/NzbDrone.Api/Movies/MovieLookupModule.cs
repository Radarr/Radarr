using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using System.Linq;
using System;
using NzbDrone.Api.REST;

namespace NzbDrone.Api.Movies
{
    public class MovieLookupModule : NzbDroneRestModule<MovieResource>
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IProvideMovieInfo _movieInfo;

        public MovieLookupModule(ISearchForNewMovie searchProxy, IProvideMovieInfo movieInfo)
            : base("/movie/lookup")
        {
            _movieInfo = movieInfo;
            _searchProxy = searchProxy;
            Get["/"] = x => Search();
            Get["/tmdb"] = x => SearchByTmdbId();
            Get["/imdb"] = x => SearchByImdbId();
        }

        private Response SearchByTmdbId()
        {
            int tmdbId = -1;
            if(Int32.TryParse(Request.Query.tmdbId, out tmdbId))
            {
                var result = _movieInfo.GetMovieInfo(tmdbId, null, true);
                return result.ToResource().AsResponse();
            }

            throw new BadRequestException("Tmdb Id was not valid");
        }

        private Response SearchByImdbId()
        {
            string imdbId = Request.Query.imdbId;
            var result = _movieInfo.GetMovieInfo(imdbId);
            return result.ToResource().AsResponse();
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
