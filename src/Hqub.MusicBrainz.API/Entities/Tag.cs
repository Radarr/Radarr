using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("tag", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class Tag
    {
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        [XmlAttribute("count")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }
    }
}
