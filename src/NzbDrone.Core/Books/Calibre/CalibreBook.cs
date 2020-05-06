using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Books.Calibre
{
    public class CalibreBook
    {
        [JsonProperty("format_metadata")]
        public Dictionary<string, CalibreBookFormat> Formats { get; set; }

        [JsonProperty("author_sort")]
        public string AuthorSort { get; set; }

        public string Title { get; set; }

        public string Series { get; set; }

        [JsonProperty("series_index")]
        public string Position { get; set; }

        public Dictionary<string, string> Identifiers { get; set; }
    }

    public class CalibreBookFormat
    {
        public string Path { get; set; }

        public long Size { get; set; }

        [JsonProperty("mtime")]
        public DateTime LastModified { get; set; }
    }
}
