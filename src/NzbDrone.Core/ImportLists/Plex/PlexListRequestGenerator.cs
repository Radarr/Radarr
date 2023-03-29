using System.Collections.Generic;
using NzbDrone.Core.Notifications.Plex.PlexTv;

namespace NzbDrone.Core.ImportLists.Plex
{
    public class PlexListRequestGenerator : IImportListRequestGenerator
    {
        private readonly IPlexTvService _plexTvService;
        private readonly int _pageSize;
        public PlexListSettings Settings { get; set; }

        public PlexListRequestGenerator(IPlexTvService plexTvService, int pageSize)
        {
            _plexTvService = plexTvService;
            _pageSize = pageSize;
        }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            var maxPages = 10;

            for (var page = 0; page < maxPages; page++)
            {
                yield return new ImportListRequest(_plexTvService.GetWatchlist(Settings.AccessToken, _pageSize, page * _pageSize));
            }
        }
    }
}
