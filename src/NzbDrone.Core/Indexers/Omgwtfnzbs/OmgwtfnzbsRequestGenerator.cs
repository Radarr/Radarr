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

        public virtual IndexerPageableRequestChain GetSearchRequests(AlbumSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();


            pageableRequests.Add(GetPagedRequests(string.Format("{0}+{1}",
                searchCriteria.ArtistQuery,
                searchCriteria.AlbumQuery)));


            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(ArtistSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();


            pageableRequests.Add(GetPagedRequests(string.Format("{0}",
                searchCriteria.ArtistQuery)));


            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string query)
        {
            var url = new StringBuilder();

            // Category 22 is Music-FLAC, category 7 is Music-MP3
            url.AppendFormat("{0}?catid=22,7&user={1}&api={2}&eng=1&delay={3}", BaseUrl, Settings.Username, Settings.ApiKey, Settings.Delay);

            if (query.IsNotNullOrWhiteSpace())
            {
                url = url.Replace("rss-download.php", "rss-search.php");
                url.AppendFormat("&search={0}", query);
            }

            yield return new IndexerRequest(url.ToString(), HttpAccept.Rss);
        }
    }
}
