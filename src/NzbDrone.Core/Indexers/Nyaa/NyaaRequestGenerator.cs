using System.Collections.Generic;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Nyaa
{
    public class NyaaRequestGenerator : IIndexerRequestGenerator
    {
        public NyaaSettings Settings { get; set; }

        public int MaxPages { get; set; }
        public int PageSize { get; set; }

        public NyaaRequestGenerator()
        {
            MaxPages = 30;
            PageSize = 100;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages, null));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AlbumSearchCriteria searchCriteria)
        {
            throw new System.NotImplementedException();
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(ArtistSearchCriteria searchCriteria)
        {
            throw new System.NotImplementedException();
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, string term)
        {
            var baseUrl = string.Format("{0}/?page=rss{1}", Settings.BaseUrl.TrimEnd('/'), Settings.AdditionalParameters);

            if (term != null)
            {
                baseUrl += "&term=" + term;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest(baseUrl, HttpAccept.Rss);
            }
            else
            {
                yield return new IndexerRequest(baseUrl, HttpAccept.Rss);

                for (var page = 1; page < maxPages; page++)
                {
                    yield return new IndexerRequest($"{baseUrl}&offset={page + 1}", HttpAccept.Rss);
                }
            }
        }

        private string PrepareQuery(string query)
        {
            return query.Replace(' ', '+');
        }
    }
}
