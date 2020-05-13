using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using Readarr.Http;

namespace Readarr.Api.V1.Artist
{
    public class ArtistLookupModule : ReadarrRestModule<ArtistResource>
    {
        private readonly ISearchForNewAuthor _searchProxy;

        public ArtistLookupModule(ISearchForNewAuthor searchProxy)
            : base("/artist/lookup")
        {
            _searchProxy = searchProxy;
            Get("/", x => Search());
        }

        private object Search()
        {
            var searchResults = _searchProxy.SearchForNewAuthor((string)Request.Query.term);
            return MapToResource(searchResults).ToList();
        }

        private static IEnumerable<ArtistResource> MapToResource(IEnumerable<NzbDrone.Core.Books.Author> artist)
        {
            foreach (var currentArtist in artist)
            {
                var resource = currentArtist.ToResource();
                var poster = currentArtist.Metadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                yield return resource;
            }
        }
    }
}
