using System.Collections.Generic;
using System.Xml.Serialization;

namespace NzbDrone.Core.NetImport.IMDbWatchList
{
    class IMDbWatchListAPI
    {
        [XmlRoot(ElementName = "item")]
        public class Movie
        {
            [XmlElement(ElementName = "pubDate")]
            public string PublishDate { get; set; }
            [XmlElement(ElementName = "title")]
            public string Title { get; set; }
            [XmlElement(ElementName = "link")]
            public string Link { get; set; }
            [XmlElement(ElementName = "guid")]
            public string Guid { get; set; }
            [XmlElement(ElementName = "description")]
            public string Description { get; set; }
        }

        [XmlRoot(ElementName = "channel")]
        public class Channel
        {
            [XmlElement(ElementName = "title")]
            public string Title { get; set; }
            [XmlElement(ElementName = "link")]
            public string Link { get; set; }
            [XmlElement(ElementName = "description")]
            public string Description { get; set; }
            [XmlElement(ElementName = "pubDate")]
            public string PublishDate { get; set; }
            [XmlElement(ElementName = "lastBuildDate")]
            public string LastBuildDate { get; set; }
            [XmlElement(ElementName = "item")]
            public List<Movie> Movie { get; set; }
        }
    }
}
