using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Trakt.Resource
{
    public class TraktCollectMovie : TraktMovieResource
    {
        [JsonProperty(PropertyName = "collected_at")]
        public DateTime CollectedAt { get; set; }
        public string Resolution { get; set; }
        public string Hdr { get; set; }

        [JsonProperty(PropertyName = "audio_channels")]
        public string AudioChannels { get; set; }
        public string Audio { get; set; }

        [JsonProperty(PropertyName = "media_type")]
        public string MediaType { get; set; }

        [JsonProperty(PropertyName = "3d")]
        public bool Is3D { get; set; }
    }
}
