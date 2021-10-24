using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IConfigService _configService;

        public MovieLookupController(ISearchForNewMovie searchProxy,
                                 IProvideMovieInfo movieInfo,
                                 IBuildFileNames fileNameBuilder,
                                 IMapCoversToLocal coverMapper,
                                 IConfigService configService)
        {
            _movieInfo = movieInfo;
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            _coverMapper = coverMapper;
            _configService = configService;
        }

        protected override MovieResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("tmdb")]
        public object SearchByTmdbId(int tmdbId)
        {
            var availDelay = _configService.AvailabilityDelay;
            var result = _movieInfo.GetMovieInfo(tmdbId).Item1;
            var translation = result.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);
            return result.ToResource(availDelay, translation);
        }

        [HttpGet("imdb")]
        public object SearchByImdbId(string imdbId)
        {
            var result = _movieInfo.GetMovieByImdbId(imdbId);

            var availDelay = _configService.AvailabilityDelay;
            var translation = result.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);
            return result.ToResource(availDelay, translation);
        }

        [HttpGet]
        public object Search([FromQuery] string term)
        {
            var searchResults = _searchProxy.SearchForNewMovie(term);

            return MapToResource(searchResults);
        }

        private IEnumerable<MovieResource> MapToResource(IEnumerable<Movie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var availDelay = _configService.AvailabilityDelay;
                var translation = currentMovie.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);
                var resource = currentMovie.ToResource(availDelay, translation);

                _coverMapper.ConvertToLocalUrls(resource.Id, resource.Images);

                var poster = currentMovie.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.RemoteUrl;
                }

                resource.Folder = _fileNameBuilder.GetMovieFolder(currentMovie);

                yield return resource;
            }
        }
    }
}
