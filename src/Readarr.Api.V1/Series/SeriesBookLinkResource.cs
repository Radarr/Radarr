using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Music;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Series
{
    public class SeriesBookLinkResource : RestResource
    {
        public string Position { get; set; }
        public int SeriesId { get; set; }
        public int BookId { get; set; }
    }

    public static class SeriesBookLinkResourceMapper
    {
        public static SeriesBookLinkResource ToResource(this SeriesBookLink model)
        {
            return new SeriesBookLinkResource
            {
                Id = model.Id,
                Position = model.Position,
                SeriesId = model.SeriesId,
                BookId = model.BookId
            };
        }

        public static List<SeriesBookLinkResource> ToResource(this IEnumerable<SeriesBookLink> models)
        {
            return models?.Select(ToResource).ToList();
        }
    }
}
