using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class BulkResource
    {
        public List<AuthorSummaryResource> AuthorMetadata { get; set; } = new List<AuthorSummaryResource>();
        public List<BookResource> Books { get; set; }
        public List<SeriesResource> Series { get; set; }
    }
}
