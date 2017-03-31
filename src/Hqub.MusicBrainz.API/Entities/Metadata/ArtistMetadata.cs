using System.Xml.Serialization;
using Hqub.MusicBrainz.API.Entities.Collections;

namespace Hqub.MusicBrainz.API.Entities.Metadata
{
    [XmlRoot("metadata", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class ArtistMetadata : MetadataWrapper
    {
        /// <summary>
        /// Gets or sets the artist-list collection.
        /// </summary>
        [XmlElement("artist-list")]
        public ArtistList Collection { get; set; }
    }
}
