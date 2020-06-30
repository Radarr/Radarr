using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// This class models the best book in a work, as defined by the Goodreads API.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class BookSearchResultResource : GoodreadsResource
    {
        public override string ElementName => "search";

        public List<WorkResource> Results { get; private set; }

        public override void Parse(XElement element)
        {
            var results = element.Descendants("results");
            if (results.Count() == 1)
            {
                Results = results.First().ParseChildren<WorkResource>();
            }
        }
    }
}
