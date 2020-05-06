using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Albums
{
    public class RenameTrackModule : ReadarrRestModule<RenameTrackResource>
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
            int authorId;

            if (Request.Query.AuthorId.HasValue)
            {
                authorId = (int)Request.Query.AuthorId;
            }
            else
            {
                throw new BadRequestException("authorId is missing");
            }

            if (Request.Query.bookId.HasValue)
            {
                var bookId = (int)Request.Query.bookId;
                return _renameTrackFileService.GetRenamePreviews(authorId, bookId).ToResource();
            }

            return _renameTrackFileService.GetRenamePreviews(authorId).ToResource();
        }
    }
}
