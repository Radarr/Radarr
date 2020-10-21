using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Simkl.Resource
{
    public class SimklCollectMovie : SimklMovieResource
    {
        [JsonProperty(PropertyName = "collected_at")]
        public DateTime CollectedAt { get; set; }
    }
}
