using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MediaCover;
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
        private readonly IMapCoversToLocal _coverMapper;

        public CreditController(ICreditService creditService, IMovieService movieService, IMapCoversToLocal coverMapper)
        {
            _creditService = creditService;
            _movieService = movieService;
            _coverMapper = coverMapper;
        }

        protected override CreditResource GetResourceById(int id)
        {
            return _creditService.GetById(id).ToResource();
        }

        [HttpGet]
        public Ok<IEnumerable<CreditResource>> GetCredits(int? movieId, int? movieMetadataId)
        {
            if (movieMetadataId.HasValue)
            {
                return TypedResults.Ok(MapToResource(_creditService.GetAllCreditsForMovieMetadata(movieMetadataId.Value)));
            }

            if (movieId.HasValue)
            {
                var movie = _movieService.GetMovie(movieId.Value);

                return TypedResults.Ok(MapToResource(_creditService.GetAllCreditsForMovieMetadata(movie.MovieMetadataId)));
            }

            return TypedResults.Ok(MapToResource(_creditService.GetAllCredits()));
        }

        private IEnumerable<CreditResource> MapToResource(IEnumerable<Credit> credits)
        {
            foreach (var currentCredits in credits)
            {
                var resource = currentCredits.ToResource();
                _coverMapper.ConvertToLocalUrls(0, resource.Images);

                yield return resource;
            }
        }
    }
}
