using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("work", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class Work
    {
        /// <summary>
        /// Gets or sets the MusicBrainz id.
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the ISW code.
        /// </summary>
        [XmlElement("iswc")]
        public string ISWC { get; set; }
    }
}
