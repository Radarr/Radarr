using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using Readarr.Api.V1.Author;
using Readarr.Api.V1.Books;
using Readarr.Http;

namespace Readarr.Api.V1.Search
{
    public class SearchModule : ReadarrRestModule<SearchResource>
    {
        private readonly ISearchForNewEntity _searchProxy;

        public SearchModule(ISearchForNewEntity searchProxy)
            : base("/search")
        {
            _searchProxy = searchProxy;
            Get("/", x => Search());
        }

        private object Search()
        {
            var searchResults = _searchProxy.SearchForNewEntity((string)Request.Query.term);
            return MapToResource(searchResults).ToList();
        }

        private static IEnumerable<SearchResource> MapToResource(IEnumerable<object> results)
        {
            int id = 1;
            foreach (var result in results)
            {
                var resource = new SearchResource();
                resource.Id = id++;

                if (result is NzbDrone.Core.Books.Author)
                {
                    var author = (NzbDrone.Core.Books.Author)result;
                    resource.Author = author.ToResource();
                    resource.ForeignId = author.ForeignAuthorId;

                    var poster = author.Metadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                    if (poster != null)
                    {
                        resource.Author.RemotePoster = poster.Url;
                    }
                }
                else if (result is NzbDrone.Core.Books.Book)
                {
                    var book = (NzbDrone.Core.Books.Book)result;
                    resource.Book = book.ToResource();
                    resource.ForeignId = book.ForeignBookId;

                    var cover = book.Editions.Value.Single(x => x.Monitored).Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Cover);
                    if (cover != null)
                    {
                        resource.Book.RemoteCover = cover.Url;
                    }
                }
                else
                {
                    throw new NotImplementedException("Bad response from search all proxy");
                }

                yield return resource;
            }
        }
    }
}
