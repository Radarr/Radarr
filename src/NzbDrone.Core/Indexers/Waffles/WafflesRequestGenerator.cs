using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using System.Text;
using System;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Waffles
{
    public class WafflesRequestGenerator : IIndexerRequestGenerator
    {
        public WafflesSettings Settings { get; set; }
        public int MaxPages { get; set; }

        public WafflesRequestGenerator()
        {
            MaxPages = 5;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages, null));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(AlbumSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages, string.Format("&q=artist:{0} album:{1}",searchCriteria.ArtistQuery,searchCriteria.AlbumQuery)));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(ArtistSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages, string.Format("&q=artist:{0}", searchCriteria.ArtistQuery)));

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, string query)
        { 

            var url = new StringBuilder();
        
            url.AppendFormat("{0}/browse.php?rss=1&c0=1&uid={1}&passkey={2}", Settings.BaseUrl.Trim().TrimEnd('/'), Settings.UserId, Settings.RssPasskey);

            if (query.IsNotNullOrWhiteSpace())
            {
                url.AppendFormat(query);
            }

            for (var page = 0; page < maxPages; page++)
            {
                yield return new IndexerRequest(string.Format("{0}&p={1}", url, page), HttpAccept.Rss);
            }
        }
    }
}
