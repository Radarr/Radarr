using System;
using System.Collections.Generic;
using System.Net.Http;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListRequestGenerator : IImportListRequestGenerator
    {
        private readonly ITraktProxy _traktProxy;
        public TraktListSettings Settings { get; set; }

        public TraktListRequestGenerator(ITraktProxy traktProxy)
        {
            _traktProxy = traktProxy;
        }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            var link = string.Empty;

            // Trakt slug rules:
            // - replace all special characters with a dash
            // - replaces multiple dashes with a single dash
            // - allows underscore as a valid character
            // - does not trim underscore from the end
            // - allows multiple underscores in a row
            var listName = Parser.Parser.ToUrlSlug(Settings.Listname.Trim(), true, "-", "-");
            link += $"users/{Settings.Username.Trim()}/lists/{listName}/items/movies";

            const int maxPageSize = 250;
            var itemsRemaining = Settings.Limit;
            var pageNumber = 1;

            while (itemsRemaining > 0)
            {
                var pageLimit = Math.Min(maxPageSize, itemsRemaining);
                var pagedLink = $"{link}?limit={pageLimit}&page={pageNumber}";

                yield return new ImportListRequest(_traktProxy.BuildRequest(pagedLink, HttpMethod.Get, Settings.AccessToken));

                itemsRemaining -= pageLimit;
                pageNumber++;
            }
        }
    }
}
