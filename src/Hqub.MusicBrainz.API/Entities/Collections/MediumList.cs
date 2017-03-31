using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities.Collections
{
    [XmlRoot("medium-list", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class MediumList : BaseList
    {
        /// <summary>
        /// Gets or sets the medium track count.
        /// </summary>
        /// <remarks>
        /// Only available in the result of a release search (???).
        /// </remarks>
        [XmlElement(ElementName = "track-count")]
        public int TrackCount { get; set; }

        /// <summary>
        /// Gets or sets the list of mediums.
        /// </summary>
        [XmlElement("medium")]
        public List<Medium> Items { get; set; }
    }
}
