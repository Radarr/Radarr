using System;
using Newtonsoft.Json;

namespace NzbDrone.Core.Notifications.Apprise
{
    public class ApprisePayload
    {
        public string Title { get; set; }

        public string Body { get; set; }

        [JsonProperty("type")]
        public ApprisePriority NotificationType { get; set; }

        public string Tag { get; set; }
    }
}
