using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.AlternativeTitles;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("alttitle")]
    public class AlternativeTitleController : RestController<AlternativeTitleResource>
    {
        private readonly IAlternativeTitleService _altTitleService;
        private readonly IMovieService _movieService;

        public AlternativeTitleController(IAlternativeTitleService altTitleService, IMovieService movieService)
        {
            _altTitleService = altTitleService;
            _movieService = movieService;
        }

        protected override AlternativeTitleResource GetResourceById(int id)
        {
            return _altTitleService.GetById(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<AlternativeTitleResource>> GetAltTitles(int? movieId, int? movieMetadataId)
        {
            if (movieMetadataId.HasValue)
            {
                return TypedResults.Ok(_altTitleService.GetAllTitlesForMovieMetadata(movieMetadataId.Value).ToResource());
            }

            if (movieId.HasValue)
            {
                var movie = _movieService.GetMovie(movieId.Value);
                return TypedResults.Ok(_altTitleService.GetAllTitlesForMovieMetadata(movie.MovieMetadataId).ToResource());
            }

            return TypedResults.Ok(_altTitleService.GetAllTitles().ToResource());
        }
    }
}
