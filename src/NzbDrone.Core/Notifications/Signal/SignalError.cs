using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Signal
{
    public class SignalError
    {
        public bool Ok { get; set; }

        [JsonProperty(PropertyName = "error_code")]
        public int ErrorCode { get; set; }

        public string Description { get; set; }
    }
}
