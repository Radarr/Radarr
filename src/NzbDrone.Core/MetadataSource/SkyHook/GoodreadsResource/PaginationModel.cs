using System.Diagnostics;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// Represents pagination information as returned by the Goodreads API.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class PaginationModel : GoodreadsResource
    {
        public override string ElementName => "";

        /// <summary>
        /// The item the current page starts on.
        /// </summary>
        public int Start { get; private set; }

        /// <summary>
        /// The item the current page ends on.
        /// </summary>
        public int End { get; private set; }

        /// <summary>
        /// The total number of items in the paginated list.
        /// </summary>
        public int TotalItems { get; private set; }

        public override void Parse(XElement element)
        {
            // Search results have different pagination fields for some reason...
            if (element.Name == "search")
            {
                Start = element.ElementAsInt("results-start");
                End = element.ElementAsInt("results-end");
                TotalItems = element.ElementAsInt("total-results");
                return;
            }

            var startAttribute = element.Attribute("start");
            var endAttribute = element.Attribute("end");
            var totalAttribute = element.Attribute("total");

            if (startAttribute != null &&
                endAttribute != null &&
                totalAttribute != null)
            {
                int.TryParse(startAttribute.Value, out int start);
                int.TryParse(endAttribute.Value, out int end);
                int.TryParse(totalAttribute.Value, out int total);

                Start = start;
                End = end;
                TotalItems = total;
            }
        }
    }
}
