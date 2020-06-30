using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using Readarr.Http;

namespace Readarr.Api.V1.Books
{
    public class BookLookupModule : ReadarrRestModule<BookResource>
    {
        private readonly ISearchForNewBook _searchProxy;

        public BookLookupModule(ISearchForNewBook searchProxy)
            : base("/book/lookup")
        {
            _searchProxy = searchProxy;
            Get("/", x => Search());
        }

        private object Search()
        {
            var searchResults = _searchProxy.SearchForNewBook((string)Request.Query.term, null);
            return MapToResource(searchResults).ToList();
        }

        private static IEnumerable<BookResource> MapToResource(IEnumerable<NzbDrone.Core.Books.Book> books)
        {
            foreach (var currentBook in books)
            {
                var resource = currentBook.ToResource();
                var cover = currentBook.Editions.Value.Single(x => x.Monitored).Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Cover);
                if (cover != null)
                {
                    resource.RemoteCover = cover.Url;
                }

                yield return resource;
            }
        }
    }
}
