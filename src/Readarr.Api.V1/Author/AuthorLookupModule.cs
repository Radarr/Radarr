using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using Readarr.Http;

namespace Readarr.Api.V1.Author
{
    public class AuthorLookupModule : ReadarrRestModule<AuthorResource>
    {
        private readonly ISearchForNewAuthor _searchProxy;

        public AuthorLookupModule(ISearchForNewAuthor searchProxy)
            : base("/author/lookup")
        {
            _searchProxy = searchProxy;
            Get("/", x => Search());
        }

        private object Search()
        {
            var searchResults = _searchProxy.SearchForNewAuthor((string)Request.Query.term);
            return MapToResource(searchResults).ToList();
        }

        private static IEnumerable<AuthorResource> MapToResource(IEnumerable<NzbDrone.Core.Books.Author> author)
        {
            foreach (var currentAuthor in author)
            {
                var resource = currentAuthor.ToResource();
                var poster = currentAuthor.Metadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                yield return resource;
            }
        }
    }
}
