using Newtonsoft.Json;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Profiles
{
    public class ProfileFormatItem : IEmbeddedDocument
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Id { get; set; }
        public CustomFormat Format { get; set; }
        public int Score { get; set; }
    }
}
