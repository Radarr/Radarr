using Nancy;
using NzbDrone.Api.Extensions;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Api.Music
{
    public class ArtistLookupModule : NzbDroneRestModule<ArtistResource>
    {
        private readonly ISearchForNewArtist _searchProxy;

        public ArtistLookupModule(ISearchForNewArtist searchProxy)
            : base("/artist/lookup")
        {
            _searchProxy = searchProxy;
            Get["/"] = x => Search();
        }


        private Response Search()
        {
            var iTunesResults = _searchProxy.SearchForNewArtist((string)Request.Query.term);
            return MapToResource(iTunesResults).AsResponse();
        }


        private static IEnumerable<ArtistResource> MapToResource(IEnumerable<Core.Music.Artist> artists)
        {
            foreach (var currentArtist in artists)
            {
                var resource = currentArtist.ToResource();
                var poster = currentArtist.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                if (poster != null)
                {
                    resource.RemotePoster = poster.Url;
                }

                yield return resource;
            }
        }
    }
}
