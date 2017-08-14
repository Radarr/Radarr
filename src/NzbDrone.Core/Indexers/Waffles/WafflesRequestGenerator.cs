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
        
        public virtual IndexerPageableRequestChain GetRecentRequests()
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(null));

            return pageableRequests;
        }

        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        public virtual IndexerPageableRequestChain GetSearchRequests(SingleEpisodeSearchCriteria searchCriteria)
        {
            throw new NotImplementedException ();
        }

        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        public virtual IndexerPageableRequestChain GetSearchRequests(SeasonSearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        public virtual IndexerPageableRequestChain GetSearchRequests(DailyEpisodeSearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        public virtual IndexerPageableRequestChain GetSearchRequests(AnimeEpisodeSearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        [System.Obsolete("Sonarr TV Stuff -- Shouldn't be needed for Lidarr")]
        public virtual IndexerPageableRequestChain GetSearchRequests(SpecialEpisodeSearchCriteria searchCriteria)
        {
            throw new NotImplementedException();
        }

        public IndexerPageableRequestChain GetSearchRequests(AlbumSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(string.Format("&q=artist:{0} album:{1}",searchCriteria.Artist.Name,searchCriteria.AlbumTitle)));

            return pageableRequests;
        }

        public IndexerPageableRequestChain GetSearchRequests(ArtistSearchCriteria searchCriteria)
        {
            var pageableRequests = new IndexerPageableRequestChain();

            pageableRequests.Add(GetPagedRequests(string.Format("&q=artist:{0}", searchCriteria.Artist.Name)));

            return pageableRequests;
        }

        private IEnumerable<IndexerRequest> GetPagedRequests(string query)
        { 

            var url = new StringBuilder();
        
            url.AppendFormat("{0}/browse.php?rss=1&c0=1&uid={1}&passkey={2}", Settings.BaseUrl.Trim().TrimEnd('/'), Settings.UserId, Settings.RssPasskey);

            if (query.IsNotNullOrWhiteSpace())
            {
                url.AppendFormat(query);
            }

            yield return new IndexerRequest(url.ToString(), HttpAccept.Rss);
        }
    }
}
