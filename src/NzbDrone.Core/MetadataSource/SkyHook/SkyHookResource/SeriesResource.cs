using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SeriesResource
    {
        public int GoodreadsId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<SeriesWorkLinkResource> Works { get; set; }
    }

    public class SeriesWorkLinkResource
    {
        public int GoodreadsId { get; set; }
        public string Position { get; set; }
        public bool Primary { get; set; }
    }
}
