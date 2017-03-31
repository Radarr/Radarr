using System.Xml.Serialization;
using Hqub.MusicBrainz.API.Entities.Collections;

namespace Hqub.MusicBrainz.API.Entities.Metadata
{
    [XmlRoot("metadata", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class ReleaseMetadata : MetadataWrapper
    {
        /// <summary>
        /// Gets or sets the release-list collection.
        /// </summary>
        [XmlElement("release-list")]
        public ReleaseList Collection { get; set; }
    }
}
