using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities.Collections
{
    public class BaseList
    {
        /// <summary>
        /// Gets or sets the total list items count.
        /// </summary>
        /// <remarks>
        /// This might be different form the actual list items count. If the list was
        /// generated from a search request, this property will return the total number
        /// of available items (on the server), while the number of returned items is
        /// limited by the requests 'limit' parameter (default = 25).
        /// </remarks>
        [XmlAttribute("count")]
        public int QueryCount { get; set; }

        /// <summary>
        /// Gets or sets the list offset (only available in search requests).
        /// </summary>
        [XmlAttribute("offset")]
        public int QueryOffset { get; set; }
    }
}