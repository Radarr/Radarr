using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities.Collections
{
    [XmlRoot("recording-list", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class ReleaseGroupList : BaseList
    {
        /// <summary>
        /// Gets or sets the list of release-groups.
        /// </summary>
        [XmlElement("release-group")]
        public List<ReleaseGroup> Items { get; set; }
    }
}
