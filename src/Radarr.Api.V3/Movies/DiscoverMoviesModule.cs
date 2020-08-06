using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.ImportExclusions;
using NzbDrone.Core.NetImport.ListMovies;
using NzbDrone.Core.Organizer;
using Radarr.Http;

namespace NzbDrone.Api.V3.Movies
{
    public class DiscoverMoviesModule : RadarrRestModule<DiscoverMoviesResource>
    {
        private readonly IMovieService _movieService;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IListMovieService _listMovieService;
        private readonly IImportExclusionsService _importExclusionService;

        public DiscoverMoviesModule(IMovieService movieService,
                                    IProvideMovieInfo movieInfo,
                                    IBuildFileNames fileNameBuilder,
                                    IListMovieService listMovieService,
                                    IImportExclusionsService importExclusionsService)
            : base("/movies/discover")
        {
            _movieService = movieService;
            _movieInfo = movieInfo;
            _fileNameBuilder = fileNameBuilder;
            _listMovieService = listMovieService;
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

            var realResults = new List<DiscoverMoviesResource>();

            //TODO: Set Is Recommendation
            realResults.AddRange(MapToResource(mapped.Where(x => x != null)));

            var listMovies = MapToResource(_listMovieService.GetAllListMovies()).ToList();

            var groupedListMovies = listMovies.GroupBy(x => x.TmdbId);

            listMovies = groupedListMovies.Select(x =>
            {
                var movie = x.First();

                movie.Lists = x.SelectMany(m => m.Lists).ToHashSet();

                return movie;
            }).ToList();

            realResults.AddRange(listMovies);

            //TODO: Distinct here by movie and if recommendation is true
            return realResults;
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

        private IEnumerable<DiscoverMoviesResource> MapToResource(IEnumerable<ListMovie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var resource = currentMovie.ToResource(_movieService, _importExclusionService);
                var poster = currentMovie.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                resource.Folder = _fileNameBuilder.GetMovieFolder(new Movie { Title = currentMovie.Title, Year = currentMovie.Year, ImdbId = currentMovie.ImdbId, TmdbId = currentMovie.TmdbId });

                yield return resource;
            }
        }
    }
}
