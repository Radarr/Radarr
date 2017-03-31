using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("medium")]
    public class Medium
    {
        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        [XmlElement("format")]
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets the disc-list.
        /// </summary>
        [XmlElement("disc-list")]
        public DiskList Disks { get; set; }

        /// <summary>
        /// Gets or sets the track-list.
        /// </summary>
        [XmlElement("track-list")]
        public Collections.TrackList Tracks { get; set; }
    }

    [XmlRoot("disk-list")]
    public class DiskList
    {
        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        [XmlAttribute("count")]
        public int Count { get; set; }
    }
}
