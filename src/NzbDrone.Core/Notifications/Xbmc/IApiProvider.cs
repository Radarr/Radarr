using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public interface IApiProvider
    {
        void Notify(XbmcSettings settings, string title, string message);
        void Update(XbmcSettings settings, Artist artist);
        void Clean(XbmcSettings settings);
        bool CanHandle(XbmcVersion version);
    }
}
