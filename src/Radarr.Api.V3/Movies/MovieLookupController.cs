using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        private static readonly Regex ImdbIdRegex = new Regex(@"imdb\.com/title/(?<id>tt\d+)",
                                                              RegexOptions.Compiled);
        private static readonly Regex TmdbIdRegex = new Regex(@"themoviedb\.org/movie/(?<id>\d+)",
                                                              RegexOptions.Compiled);

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
        public object SearchByTmdbId(int tmdbId)
        {
            var availDelay = _configService.AvailabilityDelay;
            var result = new Movie { MovieMetadata = _movieInfo.GetMovieInfo(tmdbId).Item1 };
            var translation = result.MovieMetadata.Value.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);
            return result.ToResource(availDelay, translation);
        }

        [HttpGet("imdb")]
        public object SearchByImdbId(string imdbId)
        {
            var result = new Movie { MovieMetadata = _movieInfo.GetMovieByImdbId(imdbId) };

            var availDelay = _configService.AvailabilityDelay;
            var translation = result.MovieMetadata.Value.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);
            return result.ToResource(availDelay, translation);
        }

        [HttpGet]
        public object Search([FromQuery] string term)
        {
            var convertedTerm = ConvertDbLinkToId(term);
            var searchResults = _searchProxy.SearchForNewMovie(convertedTerm);

            return MapToResource(searchResults);
        }

        private string ConvertDbLinkToId(string title)
        {
            var match = ImdbIdRegex.Match(title);
            if (match.Success)
            {
                return "imdb:" + match.Groups["id"].Value;
            }

            match = TmdbIdRegex.Match(title);
            if (match.Success)
            {
                return "tmdb:" + match.Groups["id"].Value;
            }

            return title;
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
