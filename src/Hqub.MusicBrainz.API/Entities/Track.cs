using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("track", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class Track
    {
        /// <summary>
        /// Gets or sets the MusicBrainz id.
        /// </summary>
        [XmlAttribute("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        [XmlElement("position")]
        public int Position { get; set; }

        // <number> is almost always same as <position>, so leaving it

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        [XmlElement("length")]
        public int Length { get; set; }

        /// <summary>
        /// Gets or sets the recording.
        /// </summary>
        [XmlElement("recording")]
        public Recording Recording { get; set; }

    }
}
