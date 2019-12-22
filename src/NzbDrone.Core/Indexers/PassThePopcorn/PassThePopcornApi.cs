using System;
using System.Collections.Generic;

namespace NzbDrone.Core.Indexers.PassThePopcorn
{
    public class Director
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class Torrent
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

    public class Movie
    {
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public string Cover { get; set; }
        public List<string> Tags { get; set; }
        public List<Director> Directors { get; set; }
        public string ImdbId { get; set; }
        public int TotalLeechers { get; set; }
        public int TotalSeeders { get; set; }
        public int TotalSnatched { get; set; }
        public long MaxSize { get; set; }
        public string LastUploadTime { get; set; }
        public List<Torrent> Torrents { get; set; }
    }

    public class PassThePopcornResponse
    {
        public string TotalResults { get; set; }
        public List<Movie> Movies { get; set; }
        public string Page { get; set; }
        public string AuthKey { get; set; }
        public string PassKey { get; set; }
    }

    public class PassThePopcornAuthResponse
    {
        public string Result { get; set; }
        public string Popcron { get; set; }
        public string AntiCsrfToken { get; set; }

    }

}
