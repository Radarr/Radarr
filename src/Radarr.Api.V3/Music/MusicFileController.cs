using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Music;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;
using BadRequestException = Radarr.Http.REST.BadRequestException;

namespace Radarr.Api.V3.Music
{
    [V3ApiController]
    public class MusicFileController : RestControllerWithSignalR<MusicFileResource, MusicFile>
    {
        private readonly IMusicFileService _musicFileService;

        public MusicFileController(IBroadcastSignalRMessage signalRBroadcaster,
                                   IMusicFileService musicFileService)
            : base(signalRBroadcaster)
        {
            _musicFileService = musicFileService;
        }

        protected override MusicFileResource GetResourceById(int id)
        {
            var musicFile = _musicFileService.GetMusicFile(id);
            return musicFile.ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public List<MusicFileResource> GetMusicFiles([FromQuery(Name = "trackId")] List<int> trackIds,
                                                     [FromQuery(Name = "albumId")] List<int> albumIds,
                                                     [FromQuery] List<int> musicFileIds)
        {
            if (!trackIds.Any() && !albumIds.Any() && !musicFileIds.Any())
            {
                throw new BadRequestException("trackId, albumId, or musicFileIds must be provided");
            }

            List<MusicFile> musicFiles;

            if (musicFileIds.Any())
            {
                musicFiles = _musicFileService.GetMusicFiles(musicFileIds);
            }
            else if (trackIds.Any())
            {
                musicFiles = _musicFileService.GetFilesByTrackIds(trackIds);
            }
            else
            {
                musicFiles = _musicFileService.GetFilesByAlbumIds(albumIds);
            }

            return musicFiles?.ToResource() ?? new List<MusicFileResource>();
        }

        [RestPutById]
        [Consumes("application/json")]
        public ActionResult<MusicFileResource> SetMusicFile([FromBody] MusicFileResource musicFileResource)
        {
            var musicFile = _musicFileService.GetMusicFile(musicFileResource.Id);

            musicFile.Quality = musicFileResource.Quality;
            musicFile.ReleaseGroup = musicFileResource.ReleaseGroup;
            musicFile.SceneName = musicFileResource.SceneName;

            _musicFileService.Update(musicFile);

            return Ok(musicFile.ToResource());
        }

        [RestDeleteById]
        public ActionResult DeleteMusicFile(int id)
        {
            var musicFile = _musicFileService.GetMusicFile(id);

            if (musicFile == null)
            {
                return NotFound();
            }

            _musicFileService.Delete(musicFile);

            return NoContent();
        }

        [HttpDelete("bulk")]
        [Consumes("application/json")]
        public ActionResult DeleteMusicFiles([FromBody] MusicFileListResource resource)
        {
            if (!resource.MusicFileIds.Any())
            {
                throw new BadRequestException("musicFileIds must be provided");
            }

            var musicFiles = _musicFileService.GetMusicFiles(resource.MusicFileIds);

            if (!musicFiles.Any())
            {
                return NoContent();
            }

            _musicFileService.Delete(musicFiles);

            return NoContent();
        }

        [HttpPut("bulk")]
        [Consumes("application/json")]
        public ActionResult<List<MusicFileResource>> SetPropertiesBulk([FromBody] List<MusicFileResource> resources)
        {
            var musicFiles = _musicFileService.GetMusicFiles(resources.Select(r => r.Id));

            if (!musicFiles.Any())
            {
                return Ok(new List<MusicFileResource>());
            }

            foreach (var musicFile in musicFiles)
            {
                var resourceFile = resources.Single(r => r.Id == musicFile.Id);

                if (resourceFile.Quality != null)
                {
                    musicFile.Quality = resourceFile.Quality;
                }

                if (resourceFile.ReleaseGroup != null)
                {
                    musicFile.ReleaseGroup = resourceFile.ReleaseGroup;
                }

                if (resourceFile.SceneName != null)
                {
                    musicFile.SceneName = resourceFile.SceneName;
                }
            }

            _musicFileService.Update(musicFiles);

            return Ok(musicFiles.ToResource());
        }
    }

    public class MusicFileListResource
    {
        public List<int> MusicFileIds { get; set; }
    }
}
