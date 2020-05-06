using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// Represents a paginated list of objects as returned by the Goodreads API,
    /// along with pagination information about the page size, current page, etc...
    /// </summary>
    /// <typeparam name="T">The type of the object in the paginated list.</typeparam>
    public class PaginatedList<T> : GoodreadsResource
    where T : GoodreadsResource, new()
    {
        public override string ElementName => "";

        /// <summary>
        /// The list of objects for the current page.
        /// </summary>
        public IReadOnlyList<T> List { get; private set; }

        /// <summary>
        /// Pagination information about the list and current page.
        /// </summary>
        public PaginationModel Pagination { get; private set; }

        public override void Parse(XElement element)
        {
            Pagination = new PaginationModel();
            Pagination.Parse(element);

            // Should have known search pagination would be different...
            if (element.Name == "search")
            {
                var results = element.Descendants("results");
                if (results.Count() == 1)
                {
                    List = results.First().ParseChildren<T>();
                }
            }
            else
            {
                List = element.ParseChildren<T>();
            }
        }
    }
}
