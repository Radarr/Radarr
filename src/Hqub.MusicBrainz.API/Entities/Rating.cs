using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("rating", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class Rating
    {
        /// <summary>
        /// Gets or sets the votes-count.
        /// </summary>
        [XmlAttribute("votes-count")]
        public int VotesCount { get; set; }

        /// <summary>
        /// Gets or sets the rating value.
        /// </summary>
        [XmlText]
        public double Value { get; set; }
    }
}
