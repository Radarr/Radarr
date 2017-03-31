using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities.Collections
{
    [XmlRoot("release-list", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class ReleaseList : BaseList
    {
        /// <summary>
        /// Gets or sets the list of releases.
        /// </summary>
        [XmlElement("release")]
        public List<Release> Items { get; set; }
    }
}
