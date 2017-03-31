using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Hqub.MusicBrainz.API.Entities.Collections;
using Hqub.MusicBrainz.API.Entities.Metadata;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("relation", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class Relation : Entity
    {
        public const string EntityName = "relation";

        /// <summary>
        /// Gets or sets the relation type.
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the relation type ID.
        /// </summary>
        [XmlAttribute("type-id")]
        public string TypeId { get; set; }

        /// <summary>
        /// Gets or sets the relation target.
        /// </summary>
        [XmlElement("target")]
        public string Target { get; set; }
    }
}
