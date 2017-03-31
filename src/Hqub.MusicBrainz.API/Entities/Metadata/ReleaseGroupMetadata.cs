using System.Xml.Serialization;
using Hqub.MusicBrainz.API.Entities.Collections;

namespace Hqub.MusicBrainz.API.Entities.Metadata
{
    [XmlRoot("metadata", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class ReleaseGroupMetadata : MetadataWrapper
    {
        /// <summary>
        /// Gets or sets the release-group collection.
        /// </summary>
        [XmlElement("release-group-list")]
        public ReleaseGroupList Collection { get; set; }
    }
}