using System.Collections.Generic;
using NzbDrone.Core.BookStats;

namespace Radarr.Api.V3.Books
{
    public class BookStatisticsResource
    {
        public int BookFileCount { get; set; }
        public long SizeOnDisk { get; set; }
        public List<string> ReleaseGroups { get; set; }
    }

    public static class BookStatisticsResourceMapper
    {
        public static BookStatisticsResource ToResource(this BookStatistics model)
        {
            if (model == null)
            {
                return null;
            }

            return new BookStatisticsResource
            {
                BookFileCount = model.BookFileCount,
                SizeOnDisk = model.SizeOnDisk,
                ReleaseGroups = model.ReleaseGroups
            };
        }
    }
}
