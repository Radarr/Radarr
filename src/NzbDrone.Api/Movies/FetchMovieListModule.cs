using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.NetImport;
using Radarr.Http;

namespace NzbDrone.Api.Movies
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

            List<Core.Movies.Movie> realResults = new List<Core.Movies.Movie>();

            /*foreach (var movie in results)
            {
                var mapped = _movieSearch.MapMovieToTmdbMovie(movie);

                if (mapped != null)
                {
                    realResults.Add(mapped);
                }
            }*/

            return MapToResource(results);
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
