using System.Collections.Generic;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class Credits
    {
        public List<CastResource> Cast { get; set; }
        public List<CrewResource> Crew { get; set; }
    }

    public class CastResource
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public string Character { get; set; }
        public int TmdbId { get; set; }
        public string CreditId { get; set; }
        public List<ImageResource> Images { get; set; }
    }

    public class CrewResource
    {
        public string Name { get; set; }
        public int Order { get; set; }
        public string Job { get; set; }
        public string Department { get; set; }
        public int TmdbId { get; set; }
        public string CreditId { get; set; }
        public List<ImageResource> Images { get; set; }
    }
}
