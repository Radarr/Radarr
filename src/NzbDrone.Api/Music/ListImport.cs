using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Extensions;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Music;

namespace NzbDrone.Api.Music
{
    public class ListImportModule : NzbDroneApiModule
    {
        private readonly IAddArtistService _artistService;

        public ListImportModule(IAddArtistService artistService)
            : base("/artist/import")
        {
            _artistService = artistService;
            Put["/"] = Artist => SaveAll();
        }

        private Response SaveAll()
        {
            var resources = Request.Body.FromJson<List<ArtistResource>>();

            var Artists = resources.Select(ArtistResource => (ArtistResource.ToModel())).Where(m => m != null).DistinctBy(m => m.ForeignArtistId).ToList();

            return _artistService.AddArtists(Artists).ToResource().AsResponse(HttpStatusCode.Accepted);
        }
    }
}