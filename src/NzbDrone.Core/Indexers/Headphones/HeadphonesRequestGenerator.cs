using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Headphones
{
    public class HeadphonesRequestGenerator : IIndexerRequestGenerator
    {
        private readonly IHeadphonesCapabilitiesProvider _capabilitiesProvider;
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public HeadphonesSettings Settings { get; set; }

        public HeadphonesRequestGenerator(IHeadphonesCapabilitiesProvider capabilitiesProvider)
        {
            _capabilitiesProvider = capabilitiesProvider;

            MaxPages = 30;
            PageSize = 100;
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search", ""));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AlbumSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

                pageableRequests.AddTier();

                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search",
                    NewsnabifyTitle($"&q={searchCriteria.ArtistQuery}+{searchCriteria.AlbumQuery}")));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(ArtistSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.AddTier();

            pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "search",
                NewsnabifyTitle($"&q={searchCriteria.ArtistQuery}")));

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, IEnumerable<int> categories, string searchType, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl =
                $"{Settings.BaseUrl.TrimEnd('/')}{Settings.ApiPath.TrimEnd('/')}?t={searchType}&cat={categoriesQuery}&extended=1";

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                var request = new IndexerRequest($"{baseUrl}{parameters}", HttpAccept.Rss);
                request.HttpRequest.AddBasicAuthentication(Settings.Username, Settings.Password);

                yield return request;
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    var request = new IndexerRequest($"{baseUrl}&offset={page * PageSize}&limit={PageSize}{parameters}", HttpAccept.Rss);
                    request.HttpRequest.AddBasicAuthentication(Settings.Username, Settings.Password);

                    yield return request;
                }
            }
        }

        private static string NewsnabifyTitle(string title)
        {
            return title.Replace("+", "%20");
        }
    }
}
