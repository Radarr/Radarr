using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Api.Tracks
{
    public class RenameTrackModule : NzbDroneRestModule<RenameTrackResource>
    {
        private readonly IRenameTrackFileService _renameTrackFileService;

        public RenameTrackModule(IRenameTrackFileService renameTrackFileService)
            : base("rename")
        {
            _renameTrackFileService = renameTrackFileService;

            GetResourceAll = GetTracks;
        }

        private List<RenameTrackResource> GetTracks()
        {
            if (!Request.Query.ArtistId.HasValue)
            {
                throw new BadRequestException("artistId is missing");
            }

            var artistId = (int)Request.Query.ArtistId;

            if (Request.Query.AlbumId.HasValue)
            {
                var albumId = (int)Request.Query.AlbumId;
                return _renameTrackFileService.GetRenamePreviews(artistId, albumId).ToResource();
            }

            return _renameTrackFileService.GetRenamePreviews(artistId).ToResource();
        }
    }
}
