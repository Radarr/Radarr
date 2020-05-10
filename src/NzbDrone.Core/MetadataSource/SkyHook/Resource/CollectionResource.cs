using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class CollectionResource
    {
        public string Name { get; set; }
        public int TmdbId { get; set; }
        public List<ImageResource> Images { get; set; }
    }
}
