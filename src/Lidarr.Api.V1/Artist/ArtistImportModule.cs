using System.Collections.Generic;
using Nancy;
using NzbDrone.Core.Music;
using Lidarr.Http;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V1.Artist
{
    public class ArtistImportModule : LidarrRestModule<ArtistResource>
    {
        private readonly IAddArtistService _addArtistService;

        public ArtistImportModule(IAddArtistService addArtistService)
            : base("/artist/import")
        {
            _addArtistService = addArtistService;
            Post("/",  x => Import());
        }


        private object Import()
        {
            var resource = Request.Body.FromJson<List<ArtistResource>>();
            var newArtists = resource.ToModel();

            return _addArtistService.AddArtists(newArtists).ToResource();
        }
    }
}
