using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using System.Linq;
using Radarr.Http;
using NzbDrone.Core.NetImport;
using Radarr.Api.V2.NetImport;
using NzbDrone.Core.Organizer;

namespace Radarr.Api.V2.Movies
{
    public class MovieDiscoverModule : RadarrRestModule<MovieResource>
    {
        private readonly IDiscoverNewMovies _searchProxy;
        private readonly INetImportFactory _netImportFactory;
        private readonly IBuildFileNames _fileNameBuilder;

        public MovieDiscoverModule(IDiscoverNewMovies searchProxy, INetImportFactory netImportFactory, IBuildFileNames fileNameBuilder)
            : base("/movies/discover")
        {
            _searchProxy = searchProxy;
            _netImportFactory = netImportFactory;
            _fileNameBuilder = fileNameBuilder;
            Get("/lists",  x => GetLists());
            Get("/{action?recommendations}",  x => Search(x.action));
        }

        private object Search(string action)
        {
            var imdbResults = _searchProxy.DiscoverNewMovies(action);
            return MapToResource(imdbResults);
        }

        private object GetLists()
        {
            var lists = _netImportFactory.Discoverable();

            return lists.Select(definition => {
                var resource = new NetImportResource();
                resource.Id = definition.Definition.Id;

                resource.Name = definition.Definition.Name;

                return resource;
            });
        }

        private IEnumerable<MovieResource> MapToResource(IEnumerable<Movie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var resource = currentMovie.ToResource();
                var poster = currentMovie.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                resource.Folder = _fileNameBuilder.GetMovieFolder(currentMovie);

                yield return resource;
            }
        }
    }
}
