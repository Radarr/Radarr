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

            // Trakt paginates by offset = (page - 1) * limit server-side, so limit must stay
            // constant across pages of the same fetch or later pages land on the wrong offset
            // (see #11563 review discussion). Only shrink it below maxPageSize when a single
            // page covers the whole Settings.Limit.
            const int maxPageSize = 250;
            var pagesNeeded = (int)Math.Ceiling((double)Settings.Limit / maxPageSize);
            var pageSize = pagesNeeded > 1 ? maxPageSize : Settings.Limit;

            for (var pageNumber = 1; pageNumber <= pagesNeeded; pageNumber++)
            {
                var pagedLink = $"{link}?limit={pageSize}&page={pageNumber}";

                yield return new ImportListRequest(_traktProxy.BuildRequest(pagedLink, HttpMethod.Get, Settings.AccessToken));
            }
        }
    }
}
