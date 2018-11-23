using Newtonsoft.Json;
using Radarr.Http.REST;

namespace Radarr.Api.V2.Profiles.Languages
{
    public class LanguageResource : RestResource
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public new int Id { get; set; }
        public string Name { get; set; }
        public string NameLower => Name.ToLowerInvariant();
    }
}
