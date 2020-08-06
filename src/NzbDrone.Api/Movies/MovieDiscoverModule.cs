using System.Collections.Generic;
using System.Linq;
using NzbDrone.Api.ImportList;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Movies;
using Radarr.Http;

namespace NzbDrone.Api.Movies
{
    public class MovieDiscoverModule : RadarrRestModule<MovieResource>
    {
        private readonly IImportListFactory _importListFactory;

        public MovieDiscoverModule(IImportListFactory importListFactory)
            : base("/movies/discover")
        {
            _importListFactory = importListFactory;
            Get("/lists", x => GetLists());
            Get("/{action?recommendations}", x => Search(x.action));
        }

        private object Search(string action)
        {
            //Return empty for now so as not to break 3rd Party
            var imdbResults = new List<Movie>();
            return MapToResource(imdbResults);
        }

        private object GetLists()
        {
            var lists = _importListFactory.Discoverable();

            return lists.Select(definition =>
            {
                var resource = new ImportListResource();
                resource.Id = definition.Definition.Id;

                resource.Name = definition.Definition.Name;

                return resource;
            });
        }

        private static IEnumerable<MovieResource> MapToResource(IEnumerable<Movie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var resource = currentMovie.ToResource();
                var poster = currentMovie.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                yield return resource;
            }
        }
    }
}
