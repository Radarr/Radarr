using System.Collections.Generic;
using System.Linq;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Books
{
    public class RenameBookResource : RestResource
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public int BookFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }

    public static class RenameBookResourceMapper
    {
        public static RenameBookResource ToResource(this NzbDrone.Core.MediaFiles.RenameBookFilePreview model)
        {
            if (model == null)
            {
                return null;
            }

            return new RenameBookResource
            {
                AuthorId = model.AuthorId,
                BookId = model.BookId,
                BookFileId = model.BookFileId,
                ExistingPath = model.ExistingPath,
                NewPath = model.NewPath
            };
        }

        public static List<RenameBookResource> ToResource(this IEnumerable<NzbDrone.Core.MediaFiles.RenameBookFilePreview> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
