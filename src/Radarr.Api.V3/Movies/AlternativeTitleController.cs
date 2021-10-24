using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Movies.AlternativeTitles;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Movies
{
    [V3ApiController("alttitle")]
    public class AlternativeTitleController : RestController<AlternativeTitleResource>
    {
        private readonly IAlternativeTitleService _altTitleService;

        public AlternativeTitleController(IAlternativeTitleService altTitleService)
        {
            _altTitleService = altTitleService;
        }

        protected override AlternativeTitleResource GetResourceById(int id)
        {
            return _altTitleService.GetById(id).ToResource();
        }

        [HttpGet]
        public List<AlternativeTitleResource> GetAltTitles(int? movieId)
        {
            if (movieId.HasValue)
            {
                return _altTitleService.GetAllTitlesForMovie(movieId.Value).ToResource();
            }

            return _altTitleService.GetAllTitles().ToResource();
        }
    }
}
