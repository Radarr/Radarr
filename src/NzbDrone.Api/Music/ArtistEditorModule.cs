using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.Music;

namespace NzbDrone.Api.Music
{
    public class ArtistEditorModule : NzbDroneApiModule
    {
        private readonly IArtistService _artistService;

        public ArtistEditorModule(IArtistService seriesService)
            : base("/artist/editor")
        {
            _artistService = seriesService;
            Put["/"] = artist => SaveAll();
        }

        private Response SaveAll()
        {
            var resources = Request.Body.FromJson<List<ArtistResource>>();

            var artist = resources.Select(artistResource => artistResource.ToModel(_artistService.GetArtist(artistResource.Id))).ToList();

            return _artistService.UpdateArtists(artist)
                                 .ToResource()
                                 .AsResponse(HttpStatusCode.Accepted);
        }
    }
}
