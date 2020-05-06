using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook
{
    public class SeriesResource
    {
        public string ForeignId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public List<SeriesBookLinkResource> BookLinks { get; set; }
    }

    public class SeriesBookLinkResource
    {
        public string BookId { get; set; }
        public bool Primary { get; set; }
    }
}
