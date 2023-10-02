using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class PassThePopcornResponse
    {
        public string TotalResults { get; set; }
        public List<PassThePopcornMovie> Movies { get; set; }
        public string Page { get; set; }
        public string AuthKey { get; set; }
        public string PassKey { get; set; }
    }

    public class PassThePopcornMovie
    {
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Cover { get; set; }
        public List<string> Tags { get; set; }
        public string ImdbId { get; set; }
        public List<PassThePopcornTorrent> Torrents { get; set; }
    }

    public class PassThePopcornTorrent
    {
        public int Id { get; set; }
        public string Quality { get; set; }
        public string Source { get; set; }
        public string Container { get; set; }
        public string Codec { get; set; }
        public string Resolution { get; set; }
        public bool Scene { get; set; }
        public string Size { get; set; }
        public DateTime UploadTime { get; set; }
        public string RemasterTitle { get; set; }
        public string Snatched { get; set; }
        public string Seeders { get; set; }
        public string Leechers { get; set; }
        public string ReleaseName { get; set; }
        public bool Checked { get; set; }
        public bool GoldenPopcorn { get; set; }
        public string FreeleechType { get; set; }
    }
}
