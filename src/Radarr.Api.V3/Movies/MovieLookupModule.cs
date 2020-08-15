using System.Collections.Generic;
using System.Linq;
using Nancy;
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
    public class MovieLookupModule : RadarrRestModule<MovieResource>
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;
        private readonly IMapCoversToLocal _coverMapper;
        private readonly IConfigService _configService;

        public MovieLookupModule(ISearchForNewMovie searchProxy,
                                 IProvideMovieInfo movieInfo,
                                 IBuildFileNames fileNameBuilder,
                                 IMapCoversToLocal coverMapper,
                                 IConfigService configService)
            : base("/movie/lookup")
        {
            _movieInfo = movieInfo;
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            _coverMapper = coverMapper;
            _configService = configService;
            Get("/", x => Search());
            Get("/tmdb", x => SearchByTmdbId());
            Get("/imdb", x => SearchByImdbId());
        }

        private object SearchByTmdbId()
        {
            int tmdbId = -1;
            if (int.TryParse(Request.Query.tmdbId, out tmdbId))
            {
                var result = _movieInfo.GetMovieInfo(tmdbId).Item1;
                var translation = result.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);
                return result.ToResource(translation);
            }

            throw new BadRequestException("Tmdb Id was not valid");
        }

        private object SearchByImdbId()
        {
            string imdbId = Request.Query.imdbId;
            var result = _movieInfo.GetMovieByImdbId(imdbId);

            var translation = result.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);
            return result.ToResource(translation);
        }

        private object Search()
        {
            var searchResults = _searchProxy.SearchForNewMovie((string)Request.Query.term);

            return MapToResource(searchResults);
        }

        private IEnumerable<MovieResource> MapToResource(IEnumerable<Movie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var translation = currentMovie.Translations.FirstOrDefault(t => t.Language == (Language)_configService.MovieInfoLanguage);
                var resource = currentMovie.ToResource(translation);

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
