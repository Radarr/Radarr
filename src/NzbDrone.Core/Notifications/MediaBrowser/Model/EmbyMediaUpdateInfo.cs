using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.MediaBrowser.Model
{
    public class EmbyMediaUpdateInfo
    {
        public string Path { get; set; }
        public string UpdateType { get; set; }
    }
}
