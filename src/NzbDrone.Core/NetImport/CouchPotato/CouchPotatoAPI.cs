using System.Collections.Generic;

namespace NzbDrone.Core.NetImport.CouchPotato
{
    public class CouchPotatoResponse
    {
        public Movie[] movies { get; set; }
        public int total { get; set; }
        public bool empty { get; set; }
        public bool success { get; set; }
    }

    public class Movie
    {
        public string status { get; set; }
        public Info info { get; set; }
        public string _t { get; set; }
        public List<Release> releases { get; set; }
        public string title { get; set; }
        public string _rev { get; set; }
        public string profile_id { get; set; }
        public string _id { get; set; }
        public object category_id { get; set; }
        public string type { get; set; }
    }

    public class Info
    {
        public string[] genres { get; set; }
        public int? tmdb_id { get; set; }
        public string plot { get; set; }
        public string tagline { get; set; }
        public int? year { get; set; }
        public string original_title { get; set; }
        public bool? via_imdb { get; set; }
        public string[] directors { get; set; }
        public string[] titles { get; set; }
        public string imdb { get; set; }
        public string mpaa { get; set; }
        public bool? via_tmdb { get; set; }
        public string[] actors { get; set; }
        public string[] writers { get; set; }

        //public int? runtime { get; set; }
        public string type { get; set; }
        public string released { get; set; }
    }

    public class ReleaseInfo
    {
        public double? size { get; set; }
        public int? seeders { get; set; }
        public string protocol { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public int? age { get; set; }
        public string id { get; set; }
        public int? leechers { get; set; }
        public int? score { get; set; }
        public string provider { get; set; }
        public int? seed_time { get; set; }
        public string provider_extra { get; set; }
        public string detail_url { get; set; }
        public string type { get; set; }
        public double? seed_ratio { get; set; }
        public string name { get; set; }
    }

    public class DownloadInfo
    {
        public bool? status_support { get; set; }
        public string id { get; set; }
        public string downloader { get; set; }
    }

    public class Release
    {
        public string status { get; set; }
        public ReleaseInfo info { get; set; }
        public DownloadInfo download_info { get; set; }
        public string _id { get; set; }
        public string media_id { get; set; }
        public string _rev { get; set; }
        public string _t { get; set; }
        public bool? is_3d { get; set; }
        public int? last_edit { get; set; }
        public string identifier { get; set; }
        public string quality { get; set; }
    }
}
