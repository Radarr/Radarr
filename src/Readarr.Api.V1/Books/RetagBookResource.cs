using System.Collections.Generic;
using System.Linq;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Books
{
    public class TagDifference
    {
        public string Field { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public class RetagBookResource : RestResource
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public List<int> TrackNumbers { get; set; }
        public int BookFileId { get; set; }
        public string Path { get; set; }
        public List<TagDifference> Changes { get; set; }
    }

    public static class RetagTrackResourceMapper
    {
        public static RetagBookResource ToResource(this NzbDrone.Core.MediaFiles.RetagBookFilePreview model)
        {
            if (model == null)
            {
                return null;
            }

            return new RetagBookResource
            {
                AuthorId = model.AuthorId,
                BookId = model.BookId,
                TrackNumbers = model.TrackNumbers.ToList(),
                BookFileId = model.BookFileId,
                Path = model.Path,
                Changes = model.Changes.Select(x => new TagDifference
                {
                    Field = x.Key,
                    OldValue = x.Value.Item1,
                    NewValue = x.Value.Item2
                }).ToList()
            };
        }

        public static List<RetagBookResource> ToResource(this IEnumerable<NzbDrone.Core.MediaFiles.RetagBookFilePreview> models)
        {
            return models.Select(ToResource).ToList();
        }
    }
}
