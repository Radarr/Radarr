using System.Collections.Generic;
using Nancy;
using NzbDrone.Core.Books;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Author
{
    public class AuthorImportModule : ReadarrRestModule<AuthorResource>
    {
        private readonly IAddAuthorService _addAuthorService;

        public AuthorImportModule(IAddAuthorService addAuthorService)
            : base("/author/import")
        {
            _addAuthorService = addAuthorService;
            Post("/", x => Import());
        }

        private object Import()
        {
            var resource = Request.Body.FromJson<List<AuthorResource>>();
            var newAuthors = resource.ToModel();

            return _addAuthorService.AddAuthors(newAuthors).ToResource();
        }
    }
}
