using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using Radarr.Http;

namespace Radarr.Api.V4.Movies;

[V4ApiController("movie/lookup")]
public class MovieLookupController : Controller
{
    private readonly ISearchForNewMovie _searchProxy;
    private readonly IProvideMovieInfo _movieInfo;
    private readonly IBuildFileNames _fileNameBuilder;
    private readonly INamingConfigService _namingService;
    private readonly IMapCoversToLocal _coverMapper;
    private readonly IConfigService _configService;
    private readonly IImportListExclusionService _importListExclusionService;

    public MovieLookupController(ISearchForNewMovie searchProxy,
        IProvideMovieInfo movieInfo,
        IBuildFileNames fileNameBuilder,
        INamingConfigService namingService,
        IMapCoversToLocal coverMapper,
        IConfigService configService,
        IImportListExclusionService importListExclusionService)
    {
        _movieInfo = movieInfo;
        _searchProxy = searchProxy;
        _fileNameBuilder = fileNameBuilder;
        _namingService = namingService;
        _coverMapper = coverMapper;
        _configService = configService;
        _importListExclusionService = importListExclusionService;
    }

    [HttpGet("tmdb")]
    [Produces("application/json")]
    public MovieResource SearchByTmdbId(int tmdbId)
    {
        var availabilityDelay = _configService.AvailabilityDelay;
        var result = new Movie { MovieMetadata = _movieInfo.GetMovieInfo(tmdbId).Item1 };
        var translation = result.MovieMetadata.Value.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);

        return result.ToResource(availabilityDelay, translation);
    }

    [HttpGet("imdb")]
    [Produces("application/json")]
    public MovieResource SearchByImdbId(string imdbId)
    {
        var result = new Movie { MovieMetadata = _movieInfo.GetMovieByImdbId(imdbId) };

        var availabilityDelay = _configService.AvailabilityDelay;
        var translation = result.MovieMetadata.Value.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);

        return result.ToResource(availabilityDelay, translation);
    }

    [HttpGet]
    [Produces("application/json")]
    public IEnumerable<MovieResource> Search([FromQuery] string term)
    {
        var searchResults = _searchProxy.SearchForNewMovie(term);

        return MapToResource(searchResults);
    }

    private IEnumerable<MovieResource> MapToResource(IEnumerable<Movie> movies)
    {
        var movieInfoLanguage = (Language)_configService.MovieInfoLanguage;
        var availabilityDelay = _configService.AvailabilityDelay;
        var namingConfig = _namingService.GetConfig();

        var listExclusions = _importListExclusionService.All();

        foreach (var currentMovie in movies)
        {
            var translation = currentMovie.MovieMetadata.Value.Translations.FirstOrDefault(t => t.Language == movieInfoLanguage);
            var resource = currentMovie.ToResource(availabilityDelay, translation);

            _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images);

            var poster = currentMovie.MovieMetadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);

            if (poster != null)
            {
                resource.RemotePoster = poster.RemoteUrl;
            }

            resource.Folder = _fileNameBuilder.GetMovieFolder(currentMovie, namingConfig);
            resource.IsExcluded = listExclusions.Any(e => e.TmdbId == resource.TmdbId);

            yield return resource;
        }
    }
}
