using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace NzbDrone.Core.NetImport.Kitsu
{
    public class User
    {
        public int id { get; set; }
    }

    public class KitsuUserResponse
    {
        public List<User> data { get; set; }
    }

    public class Meta
    {
        public int count { get; set; }
    }

    public class Attributes
    {
        public string canonicalTitle { get; set; }
        public string slug { get; set; }
        public string startDate { get; set; }
        public string subtype { get; set; }
    }

    // public class Relationships
    // {
        // public Mappings mappings
    // }

    public class Anime
    {
        public int id { get; set; }
        public Attributes attributes {get; set; }
        // public Relationships relationships { get; set; }
    }

    public class KitsuResponse
    {
        public Meta meta { get; set; }
        public List<Anime> included { get; set; }
        // public int? rank { get; set; }
        // public string listed_at { get; set; }
        // public string type { get; set; }

        // public long? play_count { get; set; }
        // public long? collected_count { get; set; }

        // public Anime anime { get; set; }
    }
}
