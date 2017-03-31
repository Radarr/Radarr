using System;
using System.Xml.Serialization;

namespace Hqub.MusicBrainz.API.Entities
{
    [XmlRoot("cover-art-archive", Namespace = "http://musicbrainz.org/ns/mmd-2.0#")]
    public class CoverArtArchive
    {
        /// <summary>
        /// Gets or sets a value indicating whether artwork is available or not.
        /// </summary>
        [XmlElement("artwork")]
        public bool Artwork { get; set; }

        /// <summary>
        /// Gets or sets the count.
        /// </summary>
        [XmlElement("count")]
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a front crover is available or not.
        /// </summary>
        [XmlElement("front")]
        public bool Front { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a back crover is available or not.
        /// </summary>
        [XmlElement("back")]
        public bool Back { get; set; }

        public static Uri GetCoverArtUri(string releaseId)
        {
            string url = "http://coverartarchive.org/release/" + releaseId + "/front-250.jpg";
            return new Uri(url, UriKind.RelativeOrAbsolute);
        }
    }
}
