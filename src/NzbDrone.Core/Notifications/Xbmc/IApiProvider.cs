using NzbDrone.Core.Notifications.Xbmc.Model;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public interface IApiProvider
    {
        void Notify(XbmcSettings settings, string title, string message);
        void UpdateMovie(XbmcSettings settings, Movie movie);
        void Clean(XbmcSettings settings);
        bool CanHandle(XbmcVersion version);
    }
}
