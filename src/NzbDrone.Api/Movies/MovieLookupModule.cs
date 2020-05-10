using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using Radarr.Http;
using Radarr.Http.REST;

namespace NzbDrone.Api.Movies
{
    public class MovieLookupModule : RadarrRestModule<MovieResource>
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IProvideMovieInfo _movieInfo;

        public MovieLookupModule(ISearchForNewMovie searchProxy, IProvideMovieInfo movieInfo)
            : base("/movie/lookup")
        {
            _movieInfo = movieInfo;
            _searchProxy = searchProxy;
            Get("/", x => Search());
            Get("/tmdb", x => SearchByTmdbId());
            Get("/imdb", x => SearchByImdbId());
        }

        private object SearchByTmdbId()
        {
            int tmdbId = -1;
            if (int.TryParse(Request.Query.tmdbId, out tmdbId))
            {
                var result = _movieInfo.GetMovieInfo(tmdbId).Item1;
                return result.ToResource();
            }

            throw new BadRequestException("Tmdb Id was not valid");
        }

        private object SearchByImdbId()
        {
            string imdbId = Request.Query.imdbId;
            var result = _movieInfo.GetMovieByImdbId(imdbId);
            return result.ToResource();
        }

        private object Search()
        {
            var imdbResults = _searchProxy.SearchForNewMovie((string)Request.Query.term);
            return MapToResource(imdbResults);
        }

        private static IEnumerable<MovieResource> MapToResource(IEnumerable<Core.Movies.Movie> movies)
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
