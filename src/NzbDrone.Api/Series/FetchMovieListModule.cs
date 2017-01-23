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

        public FetchMovieListModule(IFetchNetImport netImport)
            : base("/netimport/movies")
        {
            _fetchNetImport = netImport;
            Get["/"] = x => Search();
        }


        private Response Search()
        {
            var results = _fetchNetImport.FetchAndFilter((int) Request.Query.listId, false);
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