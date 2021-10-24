using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies.Credits;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Credits
{
    [V3ApiController]
    public class CreditController : RestController<CreditResource>
    {
        private readonly ICreditService _creditService;

        public CreditController(ICreditService creditService)
        {
            _creditService = creditService;
        }

        protected override CreditResource GetResourceById(int id)
        {
            return _creditService.GetById(id).ToResource();
        }

        [HttpGet]
        public List<CreditResource> GetCredits(int? movieId)
        {
            if (movieId.HasValue)
            {
                return _creditService.GetAllCreditsForMovie(movieId.Value).ToResource();
            }

            return _creditService.GetAllCredits().ToResource();
        }
    }
}
