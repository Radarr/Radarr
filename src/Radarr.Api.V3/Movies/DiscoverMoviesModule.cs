using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.ImportExclusions;
using NzbDrone.Core.Organizer;
using Radarr.Http;

namespace Radarr.Api.V3.Movies
{
    public class DiscoverMoviesModule : RadarrRestModule<DiscoverMoviesResource>
    {
        private readonly IMovieService _movieService;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IImportExclusionsService _importExclusionService;

        public DiscoverMoviesModule(IMovieService movieService, IProvideMovieInfo movieInfo, IBuildFileNames fileNameBuilder, IImportExclusionsService importExclusionsService)
            : base("/movies/discover")
        {
            _movieService = movieService;
            _movieInfo = movieInfo;
            _fileNameBuilder = fileNameBuilder;
            _importExclusionService = importExclusionsService;
            Get("/", x => GetDiscoverMovies());
        }

        private object GetDiscoverMovies()
        {
            var results = _movieService.GetRecommendedMovies();

            var mapped = new List<Movie>();

            if (results.Count > 0)
            {
                mapped = _movieInfo.GetBulkMovieInfo(results.Select(m => m.TmdbId).ToList());
            }

            var realResults = new List<Movie>();

            realResults.AddRange(mapped.Where(x => x != null));

            return MapToResource(realResults);
        }

        private IEnumerable<DiscoverMoviesResource> MapToResource(IEnumerable<Movie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var resource = currentMovie.ToResource(_movieService, _importExclusionService);
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
