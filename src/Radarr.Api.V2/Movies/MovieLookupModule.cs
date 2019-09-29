using System.Collections.Generic;
using Nancy;
using Radarr.Http.Extensions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using System.Linq;
using System;
using Radarr.Http;
using Radarr.Http.REST;
using NzbDrone.Core.Organizer;

namespace Radarr.Api.V2.Movies
{
    public class MovieLookupModule : RadarrRestModule<MovieResource>
    {
        private readonly ISearchForNewMovie _searchProxy;
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IBuildFileNames _fileNameBuilder;

        public MovieLookupModule(ISearchForNewMovie searchProxy, IProvideMovieInfo movieInfo, IBuildFileNames fileNameBuilder)
            : base("/movie/lookup")
        {
            _movieInfo = movieInfo;
            _searchProxy = searchProxy;
            _fileNameBuilder = fileNameBuilder;
            Get("/",  x => Search());
            Get("/tmdb",  x => SearchByTmdbId());
            Get("/imdb",  x => SearchByImdbId());
        }

        private object SearchByTmdbId()
        {
            int tmdbId = -1;
            if(Int32.TryParse(Request.Query.tmdbId, out tmdbId))
            {
                var result = _movieInfo.GetMovieInfo(tmdbId, null, true);
                return result.ToResource();
            }

            throw new BadRequestException("Tmdb Id was not valid");
        }

        private object SearchByImdbId()
        {
            string imdbId = Request.Query.imdbId;
            var result = _movieInfo.GetMovieInfo(imdbId);
            return result.ToResource();
        }

        private object Search()
        {
            var imdbResults = _searchProxy.SearchForNewMovie((string)Request.Query.term);
            return MapToResource(imdbResults);
        }

        private IEnumerable<MovieResource> MapToResource(IEnumerable<Movie> movies)
        {
            foreach (var currentMovie in movies)
            {
                var resource = currentMovie.ToResource();
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
