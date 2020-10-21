using NzbDrone.Core.Notifications.Simkl.Resource;

namespace NzbDrone.Core.Notifications.Simkl
{
    public class SimklUserResource
    {
        public string Username { get; set; }
        public SimklUserIdsResource Ids { get; set; }
    }
}
