using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.NetImport
{
    public class NetImportStatus : ProviderStatusBase
    {
        public Movie LastSyncListInfo { get; set; }
    }
}
