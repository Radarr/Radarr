using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport
{
    public interface INetImport : IProvider
    {
        bool Enabled { get; }
        bool EnableAuto { get; }

        NetImportType ListType { get; }
        NetImportFetchResult Fetch();
    }
}
