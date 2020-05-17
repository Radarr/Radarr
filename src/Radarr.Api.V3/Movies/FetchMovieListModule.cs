using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var tasks = results.Where(movie => movie.TmdbId == 0 || !movie.Images.Any() || movie.Overview.IsNullOrWhiteSpace())
                .Select(x => _movieSearch.MapMovieToTmdbMovieAsync(x));

            var realResults = results.Where(movie => movie.TmdbId != 0 && movie.Images.Any() && movie.Overview.IsNotNullOrWhiteSpace()).ToList();

            var mapped = Task.WhenAll(tasks).GetAwaiter().GetResult();

            realResults.AddRange(mapped.Where(x => x != null));

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
