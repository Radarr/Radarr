using System.Xml.Serialization;
using Hqub.MusicBrainz.API.Entities.Collections;

namespace Hqub.MusicBrainz.API.Entities.Metadata
{
    [XmlRoot("metadata", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class RecordingMetadata : MetadataWrapper
    {
        /// <summary>
        /// Gets or sets the recording-list collection.
        /// </summary>
        [XmlElement("recording-list")]
        public RecordingList Collection { get; set; }
    }
}
