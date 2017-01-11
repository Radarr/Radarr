using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.AutoImport
{
    public interface IAutoImport : IProvider
    {
        string Link { get; }
        bool Enabled { get; }

        // void OnGrab(GrabMessage grabMessage);
    }
}
