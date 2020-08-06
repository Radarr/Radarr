using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider.Status;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListStatus : ProviderStatusBase
    {
        public Movie LastSyncListInfo { get; set; }
    }
}
