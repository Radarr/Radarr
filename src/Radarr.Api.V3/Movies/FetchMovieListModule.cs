using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    public class FetchMovieListModule : RadarrRestModule<MovieResource>
    {
        private readonly IFetchNetImport _fetchNetImport;
        private readonly ISearchForNewMovie _movieSearch;

        public FetchMovieListModule(IFetchNetImport netImport, ISearchForNewMovie movieSearch)
            : base("/netimport/movies")
        {
            _fetchNetImport = netImport;
            _movieSearch = movieSearch;
            Get("/", x => Search());
        }

        private object Search()
        {
            var results = _fetchNetImport.FetchAndFilter((int)Request.Query.listId, false);

            List<Movie> realResults = new List<Movie>();

            foreach (var movie in results)
            {
                var mapped = movie;

                if (movie.TmdbId == 0 || !movie.Images.Any() || movie.Overview.IsNullOrWhiteSpace())
                {
                    mapped = _movieSearch.MapMovieToTmdbMovie(movie);
                }

                if (mapped != null)
                {
                    realResults.Add(mapped);
                }
            }

            return MapToResource(realResults);
        }

        private static IEnumerable<MovieResource> MapToResource(IEnumerable<Movie> movies)
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
