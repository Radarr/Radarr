using System.Collections.Generic;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport
{
    public interface INetImport : IProvider
    {
        bool Enabled { get; }
        bool EnableAuto { get; }

        NetImportFetchResult Fetch();
    }
}