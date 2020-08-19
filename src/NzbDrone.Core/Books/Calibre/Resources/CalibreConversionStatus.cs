using Newtonsoft.Json;

namespace NzbDrone.Core.Books.Calibre
{
    public class CalibreConversionStatus
    {
        public bool Running { get; set; }

        public bool Ok { get; set; }

        [JsonProperty("was_aborted")]
        public bool WasAborted { get; set; }

        public string Traceback { get; set; }

        public string Log { get; set; }
    }
}
