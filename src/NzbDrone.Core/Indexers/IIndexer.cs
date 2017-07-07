using System.Collections.Generic;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexer : IProvider
    {
        bool SupportsRss { get; }
        bool SupportsSearch { get; }
        DownloadProtocol Protocol { get; }
        
        IList<ReleaseInfo> FetchRecent();
        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        IList<ReleaseInfo> Fetch(SeasonSearchCriteria searchCriteria);
        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        IList<ReleaseInfo> Fetch(SingleEpisodeSearchCriteria searchCriteria);
        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        IList<ReleaseInfo> Fetch(DailyEpisodeSearchCriteria searchCriteria);
        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        IList<ReleaseInfo> Fetch(AnimeEpisodeSearchCriteria searchCriteria);
        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        IList<ReleaseInfo> Fetch(SpecialEpisodeSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(AlbumSearchCriteria searchCriteria);
    }
}