using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource;
using Readarr.Api.V1.Albums;
using Readarr.Api.V1.Artist;
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

                if (result is NzbDrone.Core.Music.Author)
                {
                    var artist = (NzbDrone.Core.Music.Author)result;
                    resource.Artist = artist.ToResource();
                    resource.ForeignId = artist.ForeignAuthorId;

                    var poster = artist.Metadata.Value.Images.FirstOrDefault(c => c.CoverType == MediaCoverTypes.Poster);
                    if (poster != null)
                    {
                        resource.Artist.RemotePoster = poster.Url;
                    }
                }
                else if (result is NzbDrone.Core.Music.Book)
                {
                    var album = (NzbDrone.Core.Music.Book)result;
                    resource.Album = album.ToResource();
                    resource.ForeignId = album.ForeignBookId;

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
