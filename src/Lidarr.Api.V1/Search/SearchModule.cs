using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using Lidarr.Http;
using Lidarr.Api.V1.Artist;
using Lidarr.Api.V1.Albums;
using System;

namespace Lidarr.Api.V1.Search
{
    public class SearchModule : LidarrRestModule<SearchResource>
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

        private static IEnumerable<SearchResource> MapToResource(IEnumerable<Object> results)
        {
            int id = 1;
            foreach (var result in results)
            {
                var resource = new SearchResource();
                resource.Id = id++;

                if (result is NzbDrone.Core.Music.Artist)
                {
                    var artist = (NzbDrone.Core.Music.Artist) result;
                    resource.Artist = artist.ToResource();
                    resource.ForeignId = artist.ForeignArtistId;

                    var poster = artist.Metadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                    if (poster != null)
                    {
                        resource.Artist.RemotePoster = poster.Url;
                    }
                }
                else if (result is NzbDrone.Core.Music.Album)
                {
                    var album = (NzbDrone.Core.Music.Album) result;
                    resource.Album = album.ToResource();
                    resource.ForeignId = album.ForeignAlbumId;

                    var cover = album.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Cover);
                    if (cover != null)
                    {
                        resource.Album.RemoteCover = cover.Url;
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
