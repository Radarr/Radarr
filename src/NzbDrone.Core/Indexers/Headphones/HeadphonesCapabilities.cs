using System.Collections.Generic;
using NzbDrone.Core.Indexers.Newznab;

namespace NzbDrone.Core.Indexers.Headphones
{
    public class HeadphonesCapabilities
    {
        public int DefaultPageSize { get; set; }
        public int MaxPageSize { get; set; }
        public string[] SupportedSearchParameters { get; set; }
        public List<NewznabCategory> Categories { get; set; }

        public HeadphonesCapabilities()
        {
            DefaultPageSize = 100;
            MaxPageSize = 100;
            SupportedSearchParameters = new[] { "q" };
            Categories = new List<NewznabCategory>();
        }
    }
}
