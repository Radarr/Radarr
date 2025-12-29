using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Music;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Music
{
    [V3ApiController("album/lookup")]
    public class AlbumLookupController : RestController<AlbumResource>
    {
        private readonly IAlbumService _albumService;

        public AlbumLookupController(IAlbumService albumService)
        {
            _albumService = albumService;
        }

        [NonAction]
        public override ActionResult<AlbumResource> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        protected override AlbumResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("foreignid")]
        [Produces("application/json")]
        public ActionResult<AlbumResource> SearchByForeignId(string foreignId)
        {
            var album = _albumService.FindByForeignId(foreignId);
            if (album == null)
            {
                return NotFound();
            }

            return album.ToResource();
        }

        [HttpGet("path")]
        [Produces("application/json")]
        public ActionResult<AlbumResource> SearchByPath(string path)
        {
            var album = _albumService.FindByPath(path);
            if (album == null)
            {
                return NotFound();
            }

            return album.ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<AlbumResource> Search([FromQuery] string term, int? artistId = null)
        {
            List<Album> albums;

            if (artistId.HasValue)
            {
                albums = _albumService.FindByArtistId(artistId.Value);
            }
            else
            {
                albums = _albumService.GetAllAlbums();
            }

            var results = new List<AlbumResource>();

            foreach (var album in albums)
            {
                if (album.Title != null &&
                    album.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(album.ToResource());
                }
            }

            return results;
        }
    }
}
