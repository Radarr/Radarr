using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("name-credit")]
    public class NameCredit
    {
        /// <summary>
        /// Gets or sets the joinphrase.
        /// </summary>
        [XmlAttribute("joinphrase")]
        public string JoinPhrase { get; set; }

        /// <summary>
        /// Gets or sets the artist.
        /// </summary>
        [XmlElement("artist")]
        public Artist Artist { get; set; }
    }
}