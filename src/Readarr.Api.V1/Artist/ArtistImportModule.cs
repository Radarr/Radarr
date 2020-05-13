using System.Collections.Generic;
using Nancy;
using NzbDrone.Core.Books;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Artist
{
    public class ArtistImportModule : ReadarrRestModule<ArtistResource>
    {
        private readonly IAddAuthorService _addAuthorService;

        public ArtistImportModule(IAddAuthorService addAuthorService)
            : base("/artist/import")
        {
            _addAuthorService = addAuthorService;
            Post("/", x => Import());
        }

        private object Import()
        {
            var resource = Request.Body.FromJson<List<ArtistResource>>();
            var newArtists = resource.ToModel();

            return _addAuthorService.AddAuthors(newArtists).ToResource();
        }
    }
}
