using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Music;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Music
{
    [V3ApiController("artist/lookup")]
    public class ArtistLookupController : RestController<ArtistResource>
    {
        private readonly IArtistService _artistService;

        public ArtistLookupController(IArtistService artistService)
        {
            _artistService = artistService;
        }

        [NonAction]
        public override ActionResult<ArtistResource> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        protected override ArtistResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("foreignid")]
        [Produces("application/json")]
        public ActionResult<ArtistResource> SearchByForeignId(string foreignId)
        {
            var artist = _artistService.FindByForeignId(foreignId);
            if (artist == null)
            {
                return NotFound();
            }

            return artist.ToResource();
        }

        [HttpGet("name")]
        [Produces("application/json")]
        public ActionResult<ArtistResource> SearchByName(string name)
        {
            var artist = _artistService.FindByName(name);
            if (artist == null)
            {
                return NotFound();
            }

            return artist.ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<ArtistResource> Search([FromQuery] string term)
        {
            var allArtists = _artistService.GetAllArtists();
            var results = new List<ArtistResource>();

            foreach (var artist in allArtists)
            {
                if (artist.Name != null &&
                    artist.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(artist.ToResource());
                }
            }

            return results;
        }
    }
}
