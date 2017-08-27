using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.IndexerSearch.Definitions;

namespace NzbDrone.Core.Indexers.Newznab
{
    public class NewznabRequestGenerator : IIndexerRequestGenerator
    {
        private readonly INewznabCapabilitiesProvider _capabilitiesProvider;
        public int MaxPages { get; set; }
        public int PageSize { get; set; }
        public NewznabSettings Settings { get; set; }

        public NewznabRequestGenerator(INewznabCapabilitiesProvider capabilitiesProvider)
        {
            _capabilitiesProvider = capabilitiesProvider;

            MaxPages = 30;
            PageSize = 100;
        }

        private bool SupportsSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedSearchParameters != null &&
                       capabilities.SupportedSearchParameters.Contains("q");
            }
        }

        private bool SupportsAudioSearch
        {
            get
            {
                var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

                return capabilities.SupportedAudioSearchParameters != null &&
                       capabilities.SupportedAudioSearchParameters.Contains("q") &&
                       capabilities.SupportedAudioSearchParameters.Contains("artist") &&
                       capabilities.SupportedAudioSearchParameters.Contains("album");
            }
        }

        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            var capabilities = _capabilitiesProvider.GetCapabilities(Settings);

            if (capabilities.SupportedAudioSearchParameters != null)
            {
                pageableRequests.Add(GetPagedRequests(MaxPages, Settings.Categories, "music", ""));
            }

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(AlbumSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            AddAudioPageableRequests(pageableRequests,
                string.Format("&artist={0}&album={1}",
                    searchCriteria.Artist.Name,
                    searchCriteria.AlbumTitle));

            return pageableRequests;
        }

        public virtual IndexerPageableRequestChain GetSearchRequests(ArtistSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            AddAudioPageableRequests(pageableRequests,
                string.Format("&artist={0}",
                    searchCriteria.Artist.Name));

            return pageableRequests;
        }

        private void AddAudioPageableRequests(IndexerPageableRequestChain chain, string parameters)
        {

            if (SupportsAudioSearch)
            {
                chain.AddTier();
                
                    chain.Add(GetPagedRequests(MaxPages, Settings.Categories, "music",
                        string.Format("&q={0}",
                        parameters)));
                
            }
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(int maxPages, IEnumerable<int> categories, string searchType, string parameters)
        {
            if (categories.Empty())
            {
                yield break;
            }

            var categoriesQuery = string.Join(",", categories.Distinct());

            var baseUrl = string.Format("{0}/api?t={1}&cat={2}&extended=1{3}", Settings.Url.TrimEnd('/'), searchType, categoriesQuery, Settings.AdditionalParameters);

            if (Settings.ApiKey.IsNotNullOrWhiteSpace())
            {
                baseUrl += "&apikey=" + Settings.ApiKey;
            }

            if (PageSize == 0)
            {
                yield return new IndexerRequest(string.Format("{0}{1}", baseUrl, parameters), HttpAccept.Rss);
            }
            else
            {
                for (var page = 0; page < maxPages; page++)
                {
                    yield return new IndexerRequest(string.Format("{0}&offset={1}&limit={2}{3}", baseUrl, page * PageSize, PageSize, parameters), HttpAccept.Rss);
                }
            }
        }

        private static string NewsnabifyTitle(string title)
        {
            return title.Replace("+", "%20");
        }
    }
}
