using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Indexers.MyAnonamouse
{
    public class MyAnonamouseResponse
    {
        public string Error { get; set; }
        public IReadOnlyCollection<MyAnonamouseTorrent> Data { get; set; }
        public string Message { get; set; }
    }

    public class MyAnonamouseTorrent
    {
        public int Id { get; set; }
        public string Title { get; set; }

        [JsonProperty(PropertyName = "author_info")]
        public string AuthorInfo { get; set; }

        public string Description { get; set; }

        [JsonProperty(PropertyName = "lang_code")]
        public string LanguageCode { get; set; }

        public string Filetype { get; set; }
        public bool Vip { get; set; }
        public bool Free { get; set; }

        [JsonProperty(PropertyName = "personal_freeleech")]
        public bool PersonalFreeLeech { get; set; }

        [JsonProperty(PropertyName = "fl_vip")]
        public bool FreeVip { get; set; }

        public string Category { get; set; }
        public string Added { get; set; }

        [JsonProperty(PropertyName = "times_completed")]
        public int Grabs { get; set; }

        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public int NumFiles { get; set; }
        public string Size { get; set; }
    }

    public class MyAnonamouseBuyFreeleechResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    public class MyAnonamouseUserDataResponse
    {
        [JsonProperty(PropertyName = "classname")]
        public string UserClass { get; set; }
    }
}
