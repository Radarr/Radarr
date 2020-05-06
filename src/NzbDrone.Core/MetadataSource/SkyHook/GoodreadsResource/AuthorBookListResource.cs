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
    public sealed class AuthorBookListResource : GoodreadsResource
    {
        public override string ElementName => "author";

        public List<BookResource> List { get; private set; }

        public override void Parse(XElement element)
        {
            var results = element.Descendants("books");
            if (results.Count() == 1)
            {
                List = results.First().ParseChildren<BookResource>();
            }
        }
    }
}
