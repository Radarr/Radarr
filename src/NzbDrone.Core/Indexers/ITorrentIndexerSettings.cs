using System.Collections.Generic;

namespace NzbDrone.Core.Indexers
{
    public interface ITorrentIndexerSettings : IIndexerSettings
    {
        int MinimumSeeders { get; set; }
        IEnumerable<int> RequiredFlags { get; set; }
    }
}
