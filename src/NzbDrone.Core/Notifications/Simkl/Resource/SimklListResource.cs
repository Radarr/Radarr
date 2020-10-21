using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Simkl.Resource
{
    public class SimklListResource
    {
        public int? Rank { get; set; }
        [JsonProperty(PropertyName = "listed_at")]
        public string ListedAt { get; set; }
        public string Type { get; set; }

        public int? Watchers { get; set; }

        public long? Revenue { get; set; }
        [JsonProperty(PropertyName = "watcher_count")]
        public long? WatcherCount { get; set; }
        [JsonProperty(PropertyName = "play_count")]
        public long? PlayCount { get; set; }
        [JsonProperty(PropertyName = "collected_count")]
        public long? CollectedCount { get; set; }

        public SimklMovieResource Movie { get; set; }
    }
}
