using System.Collections.Generic;
using System.Linq;
using Lidarr.Http;
using Lidarr.Http.REST;
using NzbDrone.Core.MediaFiles;

namespace Lidarr.Api.V1.Tracks
{
    public class RetagTrackModule : LidarrRestModule<RetagTrackResource>
    {
        private readonly IAudioTagService _audioTagService;

        public RetagTrackModule(IAudioTagService audioTagService)
            : base("retag")
        {
            _audioTagService = audioTagService;

            GetResourceAll = GetTracks;
        }

        private List<RetagTrackResource> GetTracks()
        {
            if (Request.Query.albumId.HasValue)
            {
                var albumId = (int)Request.Query.albumId;
                return _audioTagService.GetRetagPreviewsByAlbum(albumId).Where(x => x.Changes.Any()).ToResource();
            }
            else if (Request.Query.ArtistId.HasValue)
            {
                var artistId = (int)Request.Query.ArtistId;
                return _audioTagService.GetRetagPreviewsByArtist(artistId).Where(x => x.Changes.Any()).ToResource();
            }
            else
            {
                throw new BadRequestException("One of artistId or albumId must be specified");
            }

        }
    }
}
