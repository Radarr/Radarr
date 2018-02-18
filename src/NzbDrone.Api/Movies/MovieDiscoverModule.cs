using System.Collections.Generic;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using System.Linq;
using System;
using NzbDrone.Api.REST;
using NzbDrone.Core.NetImport;
using NzbDrone.Api.NetImport;

namespace NzbDrone.Api.Movies
{
    public class MovieDiscoverModule : NzbDroneRestModule<MovieResource>
    {
        private readonly IDiscoverNewMovies _searchProxy;
        private readonly INetImportFactory _netImportFactory;

        public MovieDiscoverModule(IDiscoverNewMovies searchProxy, INetImportFactory netImportFactory)
            : base("/movies/discover")
        {
            _searchProxy = searchProxy;
            _netImportFactory = netImportFactory;
            Get["/lists"] = x => GetLists();
            Get["/{action?recommendations}"] = x => Search(x.action);
        }

        private Response Search(string action)
        {
            var imdbResults = _searchProxy.DiscoverNewMovies(action);
            return MapToResource(imdbResults).AsResponse();
        }

        private Response GetLists()
        {
            var lists = _netImportFactory.Discoverable();

            return lists.Select(definition => {
                var resource = new NetImportResource();
                resource.Id = definition.Definition.Id;

                resource.Name = definition.Definition.Name;

                return resource;
            }).AsResponse();
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
