using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Organizer;
using Radarr.Api.V3.Movies;
using Radarr.Http;

namespace Radarr.Api.V3.ImportLists
{
    [V3ApiController("importlist/movie")]
    public class ImportListMoviesController : Controller
    {
        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IImportListMovieService _listMovieService;
        private readonly IImportListFactory _importListFactory;
        private readonly IImportExclusionsService _importExclusionService;
        private readonly INamingConfigService _namingService;
        private readonly IMovieTranslationService _movieTranslationService;
        private readonly IConfigService _configService;

        public ImportListMoviesController(IMovieService movieService,
                                    IAddMovieService addMovieService,
                                    IProvideMovieInfo movieInfo,
                                    IBuildFileNames fileNameBuilder,
                                    IImportListMovieService listMovieService,
                                    IImportListFactory importListFactory,
                                    IImportExclusionsService importExclusionsService,
                                    INamingConfigService namingService,
                                    IMovieTranslationService movieTranslationService,
                                    IConfigService configService)
        {
            _movieService = movieService;
            _addMovieService = addMovieService;
            _movieInfo = movieInfo;
            _fileNameBuilder = fileNameBuilder;
            _listMovieService = listMovieService;
            _importListFactory = importListFactory;
            _importExclusionService = importExclusionsService;
            _namingService = namingService;
            _movieTranslationService = movieTranslationService;
            _configService = configService;
        }

        [HttpGet]
        public object GetDiscoverMovies(bool includeRecommendations = false)
        {
            var movieLanguge = (Language)_configService.MovieInfoLanguage;

            var realResults = new List<ImportListMoviesResource>();
            var listExclusions = _importExclusionService.GetAllExclusions();
            var existingTmdbIds = _movieService.AllMovieTmdbIds();

            if (includeRecommendations)
            {
                var mapped = new List<Movie>();

                var results = _movieService.GetRecommendedTmdbIds();

                if (results.Count > 0)
                {
                    mapped = _movieInfo.GetBulkMovieInfo(results).Select(m => new Movie { MovieMetadata = m }).ToList();
                }

                realResults.AddRange(MapToResource(mapped.Where(x => x != null), movieLanguge));
                realResults.ForEach(x => x.IsRecommendation = true);
            }

            var listMovies = MapToResource(_listMovieService.GetAllForLists(_importListFactory.Enabled().Select(x => x.Definition.Id).ToList()), movieLanguge).ToList();

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

        [HttpPost]
        public object AddMovies([FromBody] List<MovieResource> resource)
        {
            var newMovies = resource.ToModel();

            return _addMovieService.AddMovies(newMovies, true).ToResource(0);
        }

        private IEnumerable<ImportListMoviesResource> MapToResource(IEnumerable<Movie> movies, Language language)
        {
            // Avoid calling for naming spec on every movie in filenamebuilder
            var namingConfig = _namingService.GetConfig();

            foreach (var currentMovie in movies)
            {
                var resource = DiscoverMoviesResourceMapper.ToResource(currentMovie);
                var poster = currentMovie.MovieMetadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                var translation = currentMovie.MovieMetadata.Value.Translations.FirstOrDefault(t => t.Language == language);

                resource.Title = translation?.Title ?? resource.Title;
                resource.Overview = translation?.Overview ?? resource.Overview;
                resource.Folder = _fileNameBuilder.GetMovieFolder(currentMovie, namingConfig);

                yield return resource;
            }
        }

        private IEnumerable<ImportListMoviesResource> MapToResource(IEnumerable<ImportListMovie> movies, Language language)
        {
            // Avoid calling for naming spec on every movie in filenamebuilder
            var namingConfig = _namingService.GetConfig();

            var translations = _movieTranslationService
                .GetAllTranslationsForLanguage(language)
                .ToDictionary(x => x.MovieMetadataId);

            foreach (var currentMovie in movies)
            {
                var resource = DiscoverMoviesResourceMapper.ToResource(currentMovie);
                var poster = currentMovie.MovieMetadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                var translation = GetTranslationFromDictionary(translations, currentMovie.MovieMetadata, language);

                resource.Title = translation?.Title ?? resource.Title;
                resource.Overview = translation?.Overview ?? resource.Overview;
                resource.Folder = _fileNameBuilder.GetMovieFolder(new Movie
                {
                    MovieMetadata = currentMovie.MovieMetadata
                }, namingConfig);

                yield return resource;
            }
        }

        private MovieTranslation GetTranslationFromDictionary(Dictionary<int, MovieTranslation> translations, MovieMetadata movie, Language configLanguage)
        {
            if (configLanguage == Language.Original)
            {
                return new MovieTranslation
                {
                    Title = movie.OriginalTitle,
                    Overview = movie.Overview
                };
            }

            translations.TryGetValue(movie.Id, out var translation);

            return translation;
        }
    }
}
