using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using System.Linq;
using NzbDrone.Core.NetImport;

namespace NzbDrone.Api.Movie
{
    public class FetchMovieListModule : NzbDroneRestModule<MovieResource>
    {
        private readonly IFetchNetImport _fetchNetImport;
        private readonly ISearchForNewMovie _movieSearch;

        public FetchMovieListModule(IFetchNetImport netImport, ISearchForNewMovie movieSearch)
            : base("/netimport/movies")
        {
            _fetchNetImport = netImport;
            _movieSearch = movieSearch;
            Get["/"] = x => Search();
        }


        private Response Search()
        {
            var results = _fetchNetImport.FetchAndFilter((int) Request.Query.listId, false);

            List<Core.Tv.Movie> realResults = new List<Core.Tv.Movie>();

            /*foreach (var movie in results)
            {
                var mapped = _movieSearch.MapMovieToTmdbMovie(movie);

                if (mapped != null)
                {
                    realResults.Add(mapped);
                }
            }*/

            return MapToResource(results).AsResponse();
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