using System.Collections.Generic;
using Nancy;
using NzbDrone.Core.Music;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Artist
{
    public class ArtistImportModule : ReadarrRestModule<ArtistResource>
    {
        private readonly IAddArtistService _addArtistService;

        public ArtistImportModule(IAddArtistService addArtistService)
            : base("/artist/import")
        {
            _addArtistService = addArtistService;
            Post("/", x => Import());
        }

        private object Import()
        {
            var resource = Request.Body.FromJson<List<ArtistResource>>();
            var newArtists = resource.ToModel();

            return _addArtistService.AddArtists(newArtists).ToResource();
        }
    }
}
