using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities.Collections
{
    [XmlRoot("artist-list", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class ArtistList : BaseList
    {
        /// <summary>
        /// Gets or sets the list of artists.
        /// </summary>
        [XmlElement("artist")]
        public List<Artist> Items { get; set; }
    }
}
