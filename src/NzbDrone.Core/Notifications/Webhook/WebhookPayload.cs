﻿using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookPayload
    {
        public string EventType { get; set; }
        public WebhookMovie Movie { get; set; }
        public WebhookRemoteMovie RemoteMovie { get; set; }
    }
}
