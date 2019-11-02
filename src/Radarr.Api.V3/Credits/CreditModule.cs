using System;
using System.Collections.Generic;
using NzbDrone.Core.Movies.Credits;
using Radarr.Http;

namespace Radarr.Api.V3.Credits
{
    public class CreditModule : RadarrRestModule<CreditResource>
    {
        private readonly ICreditService _creditService;

        public CreditModule(ICreditService creditService)
        {
            _creditService = creditService;

            GetResourceById = GetCredit;
            GetResourceAll = GetCredits;
        }

        private CreditResource GetCredit(int id)
        {
            return _creditService.GetById(id).ToResource();
        }

        private List<CreditResource> GetCredits()
        {
            var movieIdQuery = Request.Query.MovieId;

            if (movieIdQuery.HasValue)
            {
                int movieId = Convert.ToInt32(movieIdQuery.Value);

                return _creditService.GetAllCreditsForMovie(movieId).ToResource();
            }

            return _creditService.GetAllCredits().ToResource();
        }
    }
}
