using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Credits;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Credits
{
    [V3ApiController]
    public class CreditController : RestController<CreditResource>
    {
        private readonly ICreditService _creditService;
        private readonly IMovieService _movieService;

        public CreditController(ICreditService creditService, IMovieService movieService)
        {
            _creditService = creditService;
            _movieService = movieService;
        }

        protected override CreditResource GetResourceById(int id)
        {
            return _creditService.GetById(id).ToResource();
        }

        [HttpGet]
        public List<CreditResource> GetCredits(int? movieId, int? movieMetadataId)
        {
            if (movieMetadataId.HasValue)
            {
                return _creditService.GetAllCreditsForMovieMetadata(movieMetadataId.Value).ToResource();
            }

            if (movieId.HasValue)
            {
                var movie = _movieService.GetMovie(movieId.Value);
                return _creditService.GetAllCreditsForMovieMetadata(movie.MovieMetadataId).ToResource();
            }

            return _creditService.GetAllCredits().ToResource();
        }
    }
}
