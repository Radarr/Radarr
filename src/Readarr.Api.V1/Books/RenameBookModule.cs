using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Books
{
    public class RenameBookModule : ReadarrRestModule<RenameBookResource>
    {
        private readonly IRenameBookFileService _renameBookFileService;

        public RenameBookModule(IRenameBookFileService renameBookFileService)
            : base("rename")
        {
            _renameBookFileService = renameBookFileService;

            GetResourceAll = GetBookFiles;
        }

        private List<RenameBookResource> GetBookFiles()
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
                return _renameBookFileService.GetRenamePreviews(authorId, bookId).ToResource();
            }

            return _renameBookFileService.GetRenamePreviews(authorId).ToResource();
        }
    }
}
