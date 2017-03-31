using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities.Collections
{
    [XmlRoot("relation-list", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class RelationList : BaseList
    {
        /// <summary>
        /// Gets or sets the relation target type.
        /// </summary>
        [XmlAttribute("target-type")]
        public string TargetType { get; set; }

        /// <summary>
        /// Gets or sets the list of relations.
        /// </summary>
        [XmlElement("relation")]
        public List<Relation> Items { get; set; }
    }
}
