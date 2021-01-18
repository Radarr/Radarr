using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.Books.Calibre
{
        public class CalibreLibraryInfo
        {
            [JsonProperty("library_map")]
            public Dictionary<string, string> LibraryMap { get; set; }
            [JsonProperty("default_library")]
            public string DefaultLibrary { get; set; }
        }
}
