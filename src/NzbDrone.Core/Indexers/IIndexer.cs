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
        IList<ReleaseInfo> Fetch(AlbumSearchCriteria searchCriteria);
        IList<ReleaseInfo> Fetch(ArtistSearchCriteria searchCriteria);
    }
}
