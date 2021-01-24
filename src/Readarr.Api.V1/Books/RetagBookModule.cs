using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Books
{
    public class RetagBookModule : ReadarrRestModule<RetagBookResource>
    {
        private readonly IAudioTagService _audioTagService;

        public RetagBookModule(IAudioTagService audioTagService)
            : base("retag")
        {
            _audioTagService = audioTagService;

            GetResourceAll = GetBooks;
        }

        private List<RetagBookResource> GetBooks()
        {
            if (Request.Query.bookId.HasValue)
            {
                var bookId = (int)Request.Query.bookId;
                return _audioTagService.GetRetagPreviewsByBook(bookId).Where(x => x.Changes.Any()).ToResource();
            }
            else if (Request.Query.AuthorId.HasValue)
            {
                var authorId = (int)Request.Query.AuthorId;
                return _audioTagService.GetRetagPreviewsByAuthor(authorId).Where(x => x.Changes.Any()).ToResource();
            }
            else
            {
                throw new BadRequestException("One of authorId or bookId must be specified");
            }
        }
    }
}
