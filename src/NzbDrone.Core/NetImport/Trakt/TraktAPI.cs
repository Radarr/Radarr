using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class Ids
    {
        public int trakt { get; set; }
        public string slug { get; set; }
        public string imdb { get; set; }
        public int tmdb { get; set; }
    }

    public class Movie
    {
        public string title { get; set; }
        public int year { get; set; }
        public Ids ids { get; set; }
    }

    public class TraktResponse
    {
        public int rank { get; set; }
        public string listed_at { get; set; }
        public string type { get; set; }
        public Movie movie { get; set; }
    }
}
