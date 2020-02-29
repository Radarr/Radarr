using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Tracks
{
    public class RetagTrackModule : ReadarrRestModule<RetagTrackResource>
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
