using System;
using System.Collections.Generic;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers
{
    public interface IIndexerRequestGenerator
    {
        IndexerPageableRequestChain GetRecentRequests();
        IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria);
        Func<IDictionary<string, string>> GetCookies { get; set; }
        Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
