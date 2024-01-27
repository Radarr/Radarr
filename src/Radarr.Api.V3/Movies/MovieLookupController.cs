using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Organizer;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("movie/lookup")]
    public class MovieLookupController : RestController<MovieResource>
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly INamingConfigService _namingService;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IConfigService _configService;

        public MovieLookupController(ISearchForNewMovie searchProxy,
                                 IProvideMovieInfo movieInfo,
                                 IBuildFileNames fileNameBuilder,
                                 INamingConfigService namingService,
                                 IMapCoversToLocal coverMapper,
                                 IConfigService configService)
        {
            _movieInfo = movieInfo;
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            _namingService = namingService;
            _coverMapper = coverMapper;
            _configService = configService;
        }

        [NonAction]
        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("tmdb")]
        [Produces("application/json")]
        public async Task<MovieResource> SearchByTmdbId(int tmdbId)
        {
            var availDelay = _configService.AvailabilityDelay;
            var movieInfo = await _movieInfo.GetMovieInfo(tmdbId);
            var result = new Movie { MovieMetadata = movieInfo.Item1 };
            var translation = result.MovieMetadata.Value.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);

            return result.ToResource(availDelay, translation);
        }

        [HttpGet("imdb")]
        [Produces("application/json")]
        public async Task<MovieResource> SearchByImdbId(string imdbId)
        {
            var result = new Movie { MovieMetadata = await _movieInfo.GetMovieByImdbId(imdbId) };

            var availDelay = _configService.AvailabilityDelay;
            var translation = result.MovieMetadata.Value.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);

            return result.ToResource(availDelay, translation);
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<IEnumerable<MovieResource>> Search([FromQuery] string term)
        {
            var searchResults = await _searchProxy.SearchForNewMovie(term);

            return MapToResource(searchResults);
        }

        private IEnumerable<MovieResource> MapToResource(IEnumerable<Movie> movies)
        {
            var movieInfoLanguage = (Language)_configService.MovieInfoLanguage;
            var availDelay = _configService.AvailabilityDelay;
            var namingConfig = _namingService.GetConfig();

            foreach (var currentMovie in movies)
            {
                var translation = currentMovie.MovieMetadata.Value.Translations.FirstOrDefault(t => t.Language == movieInfoLanguage);
                var resource = currentMovie.ToResource(availDelay, translation);

                _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images);

                var poster = currentMovie.MovieMetadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.RemoteUrl;
                }

                resource.Folder = _fileNameBuilder.GetMovieFolder(currentMovie, namingConfig);

                yield return resource;
            }
        }
    }
}
