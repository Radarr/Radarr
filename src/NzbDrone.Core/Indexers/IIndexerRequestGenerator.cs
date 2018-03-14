using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerRequestGenerator
    {
        IndexerPageableRequestChain GetRecentRequests();
        IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria);
    }
}
