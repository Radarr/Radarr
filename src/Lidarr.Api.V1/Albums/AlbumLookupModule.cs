using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using Lidarr.Http;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V1.Albums
{
    public class AlbumLookupModule : LidarrRestModule<AlbumResource>
    {
        private readonly ISearchForNewAlbum _searchProxy;

        public AlbumLookupModule(ISearchForNewAlbum searchProxy)
            : base("/album/lookup")
        {
            _searchProxy = searchProxy;
            Get["/"] = x => Search();
        }

        private Response Search()
        {
            var searchResults = _searchProxy.SearchForNewAlbum((string)Request.Query.term, null);
            return MapToResource(searchResults).ToList().AsResponse();
        }

        private static IEnumerable<AlbumResource> MapToResource(IEnumerable<NzbDrone.Core.Music.Album> albums)
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
