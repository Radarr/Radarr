using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities.Collections
{
    [XmlRoot("work-list", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class WorkList : BaseList
    {
        /// <summary>
        /// Gets or sets the list of works.
        /// </summary>
        [XmlElement("work")]
        public List<Work> Items { get; set; }
    }
}
