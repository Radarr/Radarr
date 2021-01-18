using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Books.Calibre
{
    public class CalibreCategory
    {
        [JsonProperty("total_num")]
        public int TotalNum { get; set; }
        [JsonProperty("sort_order")]
        public string SortOrder { get; set; }
        public int Offset { get; set; }
        public int Num { get; set; }
        public string Sort { get; set; }
        [JsonProperty("base_url")]
        public string BaseUrl { get; set; }
        [JsonProperty("book_ids")]
        public List<int> BookIds { get; set; }
    }
}
