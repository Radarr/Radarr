using System;
using System.Collections.Generic;
using System.Text;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Omgwtfnzbs
{
    public class OmgwtfnzbsRequestGenerator : IIndexerRequestGenerator
    {
        public string BaseUrl { get; set; }
        public OmgwtfnzbsSettings Settings { get; set; }

        public OmgwtfnzbsRequestGenerator()
        {
            BaseUrl = "https://rss.omgwtfnzbs.me/rss-download.php";
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(null));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(MovieSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            foreach (var queryTitle in searchCriteria.CleanSceneTitles)
            {
                pageableRequests.Add(GetPagedRequests(string.Format("{0}",
                    queryTitle)));
            }

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string query)
        {
            var url = new StringBuilder();
            url.AppendFormat("{0}?catid=15,16,17,18,31,35&user={1}&api={2}&eng=1&delay={3}", BaseUrl, Settings.Username, Settings.ApiKey, Settings.Delay);

            if (query.IsNotNullOrWhiteSpace())
            {
                url = url.Replace("rss-download.php", "rss-search.php");
                url.AppendFormat("&search={0}", query);
            }

            yield return new IndexerRequest(url.ToString(), HttpAccept.Rss);
        }

        public Func<IDictionary<string, string>> GetCookies { get; set; }
        public Action<IDictionary<string, string>, DateTime?> CookiesUpdater { get; set; }
    }
}
