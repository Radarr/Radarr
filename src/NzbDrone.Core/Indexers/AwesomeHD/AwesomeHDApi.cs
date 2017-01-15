using System;
using Newtonsoft.Json;

using System.Xml.Serialization;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.AwesomeHD
{
    public class Torrent
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }
        [XmlElement(ElementName = "groupid")]
        public string GroupId { get; set; }
        [XmlElement(ElementName = "time")]
        public DateTime Time { get; set; }
        [XmlElement(ElementName = "userid")]
        public string Userid { get; set; }
        [XmlElement(ElementName = "size")]
        public long Size { get; set; }
        [XmlElement(ElementName = "snatched")]
        public string Snatched { get; set; }
        [XmlElement(ElementName = "seeders")]
        public string Seeders { get; set; }
        [XmlElement(ElementName = "leechers")]
        public string Leechers { get; set; }
        [XmlElement(ElementName = "releasegroup")]
        public string Releasegroup { get; set; }
        [XmlElement(ElementName = "resolution")]
        public string Resolution { get; set; }
        [XmlElement(ElementName = "media")]
        public string Media { get; set; }
        [XmlElement(ElementName = "format")]
        public string Format { get; set; }
        [XmlElement(ElementName = "encoding")]
        public string Encoding { get; set; }
        [XmlElement(ElementName = "audioformat")]
        public string Audioformat { get; set; }
        [XmlElement(ElementName = "audiobitrate")]
        public string Audiobitrate { get; set; }
        [XmlElement(ElementName = "audiochannels")]
        public string Audiochannels { get; set; }
        [XmlElement(ElementName = "subtitles")]
        public string Subtitles { get; set; }
        [XmlElement(ElementName = "encodestatus")]
        public string Encodestatus { get; set; }
        [XmlElement(ElementName = "freeleech")]
        public string Freeleech { get; set; }
        [XmlElement(ElementName = "cover")]
        public string Cover { get; set; }
        [XmlElement(ElementName = "smallcover")]
        public string Smallcover { get; set; }
        [XmlElement(ElementName = "year")]
        public string Year { get; set; }
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }
        [XmlElement(ElementName = "imdb")]
        public string Imdb { get; set; }
        [XmlElement(ElementName = "type")]
        public string Type { get; set; }
        [XmlElement(ElementName = "plotoutline")]
        public string Plotoutline { get; set; }
    }

    public class SearchResults
    {
        [XmlElement(ElementName = "authkey")]
        public string AuthKey { get; set; }
        [XmlElement(ElementName = "torrent")]
        public List<Torrent> Torrent { get; set; }
    }

    public class AwesomeHDSearchResponse
    {
        [XmlElement(ElementName = "?xml")]
        public string Xml { get; set; }
        [XmlElement(ElementName = "searchresults")]
        public SearchResults SearchResults { get; set; }
    }
}
