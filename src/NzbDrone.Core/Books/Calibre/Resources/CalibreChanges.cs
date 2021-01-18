using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Books.Calibre
{
    public class CalibreChangesPayload
    {
        public CalibreChanges Changes { get; set; }
        [JsonProperty("loaded_book_ids")]
        public List<int> LoadedBookIds { get; set; }
    }

    public class CalibreChanges
    {
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string Cover { get; set; }
        [JsonProperty("pubdate")]
        public DateTime? PubDate { get; set; }
        public string Publisher { get; set; }
        public string Languages { get; set; }
        public string Comments { get; set; }
        public decimal Rating { get; set; }
        public Dictionary<string, string> Identifiers { get; set; }
        public string Series { get; set; }
        [JsonProperty("series_index")]
        public double? SeriesIndex { get; set; }
        [JsonProperty("added_formats")]
        public List<CalibreAddFormat> AddedFormats { get; set; }
        [JsonProperty("removed_formats")]
        public List<string> RemovedFormats { get; set; }
    }

    public class CalibreAddFormat
    {
        public string Ext { get; set; }
        [JsonProperty("data_url")]
        public string Data { get; set; }
    }
}
