using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.ImportLists
{
    public class ImportListMoviesModule : RadarrRestModule<ImportListMoviesResource>
    {
        private readonly IMovieService _movieService;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IImportListMovieService _listMovieService;
        private readonly IImportExclusionsService _importExclusionService;

        public ImportListMoviesModule(IMovieService movieService,
                                    IProvideMovieInfo movieInfo,
                                    IBuildFileNames fileNameBuilder,
                                    IImportListMovieService listMovieService,
                                    IImportExclusionsService importExclusionsService)
            : base("/importlist/movie")
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
            var includeRecommendations = Request.GetBooleanQueryParameter("includeRecommendations");

            var realResults = new List<ImportListMoviesResource>();
            var listExclusions = _importExclusionService.GetAllExclusions();
            var existingTmdbIds = _movieService.AllMovieTmdbIds();

            if (includeRecommendations)
            {
                var mapped = new List<Movie>();

                var results = _movieService.GetRecommendedTmdbIds();

                if (results.Count > 0)
                {
                    mapped = _movieInfo.GetBulkMovieInfo(results);
                }

                realResults.AddRange(MapToResource(mapped.Where(x => x != null)));
                realResults.ForEach(x => x.IsRecommendation = true);
            }

            var listMovies = MapToResource(_listMovieService.GetAllListMovies()).ToList();

            realResults.AddRange(listMovies);

            var groupedListMovies = realResults.GroupBy(x => x.TmdbId);

            // Distinct Movies
            realResults = groupedListMovies.Select(x =>
            {
                var movie = x.First();

                movie.Lists = x.SelectMany(m => m.Lists).ToHashSet();
                movie.IsExcluded = listExclusions.Any(e => e.TmdbId == movie.TmdbId);
                movie.IsExisting = existingTmdbIds.Any(e => e == movie.TmdbId);
                movie.IsRecommendation = x.Any(m => m.IsRecommendation);

                return movie;
            }).ToList();

            return realResults;
        }

        private IEnumerable<ImportListMoviesResource> MapToResource(IEnumerable<Movie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var resource = DiscoverMoviesResourceMapper.ToResource(currentMovie);
                var poster = currentMovie.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                resource.Folder = _fileNameBuilder.GetMovieFolder(currentMovie);

                yield return resource;
            }
        }

        private IEnumerable<ImportListMoviesResource> MapToResource(IEnumerable<ImportListMovie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var resource = DiscoverMoviesResourceMapper.ToResource(currentMovie);
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
