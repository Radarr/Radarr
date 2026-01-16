using Newtonsoft.Json;

namespace NzbDrone.Core.MetadataSource.SkyHook.Resource
{
    public class TranslationResource
    {
        public string Title { get; set; }
        public string Overview { get; set; }
        public string Language { get; set; }

        // For TMDb direct API response (iso codes + nested data)
        [JsonProperty("iso_639_1")]
        public string Iso6391 { get; set; }

        [JsonProperty("iso_3166_1")]
        public string Iso31661 { get; set; }

        [JsonProperty("data")]
        public TranslationDataResource Data { get; set; }
    }

    public class TranslationDataResource
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }
    }
}
