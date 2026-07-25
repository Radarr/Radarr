using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Trakt;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserRequestGenerator : IImportListRequestGenerator
    {
        private readonly ITraktProxy _traktProxy;
        private readonly TraktUserSettings _settings;

        public TraktUserRequestGenerator(ITraktProxy traktProxy, TraktUserSettings settings)
        {
            _traktProxy = traktProxy;
            _settings = settings;
        }

        public virtual ImportListPageableRequestChain GetMovies()
        {
            var pageableRequests = new ImportListPageableRequestChain();

            pageableRequests.Add(GetMoviesRequest());

            return pageableRequests;
        }

        private IEnumerable<ImportListRequest> GetMoviesRequest()
        {
            var requestBuilder = new HttpRequestBuilder(_settings.Link.Trim());

            switch (_settings.TraktListType)
            {
                case (int)TraktUserListType.UserWatchList:
                    var watchSorting = _settings.TraktWatchSorting switch
                    {
                        (int)TraktUserWatchSorting.Added => "added",
                        (int)TraktUserWatchSorting.Title => "title",
                        (int)TraktUserWatchSorting.Released => "released",
                        _ => "rank"
                    };

                    requestBuilder
                        .Resource("/users/{userName}/watchlist/movies/{sorting}")
                        .SetSegment("sorting", watchSorting);
                    break;
                case (int)TraktUserListType.UserWatchedList:
                    requestBuilder.Resource("/users/{userName}/watched/movies");
                    break;
                case (int)TraktUserListType.UserCollectionList:
                    requestBuilder.Resource("/users/{userName}/collection/movies");
                    break;
            }

            var userName = _settings.Username.IsNotNullOrWhiteSpace() ? _settings.Username.Trim() : _settings.AuthUser.Trim();

            requestBuilder
                .SetSegment("userName", userName)
                .WithRateLimit(4);

            // Trakt paginates by offset = (page - 1) * limit server-side, so limit must stay
            // constant across pages of the same fetch or later pages land on the wrong offset
            // (see #11563 review discussion). Only shrink it below maxPageSize when a single
            // page covers the whole Settings.Limit.
            const int maxPageSize = 250;
            var pagesNeeded = (int)Math.Ceiling((double)_settings.Limit / maxPageSize);
            var pageSize = pagesNeeded > 1 ? maxPageSize : _settings.Limit;

            for (var pageNumber = 1; pageNumber <= pagesNeeded; pageNumber++)
            {
                requestBuilder
                    .AddQueryParam("page", pageNumber, true)
                    .AddQueryParam("limit", pageSize, true);

                yield return new ImportListRequest(_traktProxy.BuildRequest(requestBuilder.Build(), _settings.AccessToken));
            }
        }
    }
}
