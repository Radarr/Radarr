using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;

namespace NzbDrone.Core.MetadataSource.Goodreads
{
    /// <summary>
    /// Represents information about a book series as defined by the Goodreads API.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public sealed class SeriesResource : GoodreadsResource
    {
        public SeriesResource()
        {
            Works = new List<WorkResource>();
        }

        public override string ElementName => "series";

        /// <summary>
        /// The Id of the series.
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        /// The title of the series.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The description of the series.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Any notes for the series.
        /// </summary>
        public string Note { get; private set; }

        /// <summary>
        /// How many works are contained in the series total.
        /// </summary>
        public int SeriesWorksCount { get; private set; }

        /// <summary>
        /// The count of works that are considered primary in the series.
        /// </summary>
        public int PrimaryWorksCount { get; private set; }

        /// <summary>
        /// Determines if the series is usually numbered or not.
        /// </summary>
        public bool IsNumbered { get; private set; }

        /// <summary>
        /// The list of works that are in this series.
        /// Only populated if Goodreads returns it in the response.
        /// </summary>
        public List<WorkResource> Works { get; set; }

        public override void Parse(XElement element)
        {
            Id = element.ElementAsLong("id");
            Title = element.ElementAsString("title", true);
            Description = element.ElementAsString("description", true);
            Note = element.ElementAsString("note", true);
            SeriesWorksCount = element.ElementAsInt("series_works_count");
            PrimaryWorksCount = element.ElementAsInt("primary_work_count");
            IsNumbered = element.ElementAsBool("numbered");
        }
    }
}
