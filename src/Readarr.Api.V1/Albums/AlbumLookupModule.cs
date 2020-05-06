using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using Readarr.Http;

namespace Readarr.Api.V1.Albums
{
    public class AlbumLookupModule : ReadarrRestModule<AlbumResource>
    {
        private readonly ISearchForNewBook _searchProxy;

        public AlbumLookupModule(ISearchForNewBook searchProxy)
            : base("/album/lookup")
        {
            _searchProxy = searchProxy;
            Get("/", x => Search());
        }

        private object Search()
        {
            var searchResults = _searchProxy.SearchForNewBook((string)Request.Query.term, null);
            return MapToResource(searchResults).ToList();
        }

        private static IEnumerable<AlbumResource> MapToResource(IEnumerable<NzbDrone.Core.Music.Book> albums)
        {
            foreach (var currentAlbum in albums)
            {
                var resource = currentAlbum.ToResource();
                var cover = currentAlbum.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Cover);
                if (cover != null)
                {
                    resource.RemoteCover = cover.Url;
                }

                yield return resource;
            }
        }
    }
}
