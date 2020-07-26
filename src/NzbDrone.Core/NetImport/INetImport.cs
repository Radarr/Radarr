using System.Collections.Generic;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.NetImport
{
    public interface INetImport : IProvider
    {
        bool Enabled { get; }
        bool EnableAuto { get; }

        NetImportType ListType { get; }
        List<Movie> Fetch();
    }
}
