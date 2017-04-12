using NzbDrone.Common.Http;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;


namespace NzbDrone.Core.NetImport.Trakt
{
    public class RemoveFromListRequestData
    {
        public List<Movie> movies { get; set; }
    }

    public class RefreshRequestResponse
    {
		public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
    }

    public class TraktRequestGenerator : INetImportRequestGenerator
    {
    	public IConfigService _configService;
        public IHttpClient HttpClient { get; set; }
        public TraktSettings Settings { get; set; }

        public string RadarrTraktUrl { get; set; }

        public TraktRequestGenerator()
        {
            RadarrTraktUrl = "http://radarr.aeonlucid.com/v1/trakt/refresh?refresh=";
        }

        public virtual void Clean(NzbDrone.Core.Tv.Movie movie)
        {
            if (_configService.TraktRefreshToken != string.Empty)
            {
                var link = Settings.Link.Trim();
                bool continueRemoval = false;
                switch (Settings.ListType)
                {
                    case (int)TraktListType.UserCustomList:
                        //https://api.trakt.tv/users/id/lists/list_id/items/remove
                        var listName = Parser.Parser.ToUrlSlug(Settings.Listname.Trim());
                        link = link + $"/users/{Settings.Username.Trim()}/lists/{listName}/items/remove";
                        continueRemoval = true;
                        break;
                    case (int)TraktListType.UserWatchList:
                        //https://api.trakt.tv/sync/watchlist/remove
                        link = link + $"/sync/watchlist/remove";
                        if (true) { continueRemoval = true; } //should check if more global option to remove from watchlist is enabled
                        break;

                }
                if (continueRemoval)
                {

                    Authenticate();

                    var requestBuilder = new HttpRequestBuilder($"{link}")
                    {
                        LogResponseContent = true
                    };

                    requestBuilder.Method = HttpMethod.POST;

                    var listRemovalRequest = requestBuilder
                               .SetHeader("Content-Type", "application/json")
                               .Accept(HttpAccept.Json)
                               .SetHeader("trakt-api-version", "2")
                               .SetHeader("trakt-api-key", "964f67b126ade0112c4ae1f0aea3a8fb03190f71117bd83af6a0560a99bc52e6")
                               .SetHeader("Authorization", "Bearer " + _configService.TraktAuthToken)
                               .Build();

                    var postData = new RemoveFromListRequestData();
                    var moviesList = new List<Movie>();
                    var mov = new Movie();
                    var ids = new Ids();

                    ids.imdb = movie.ImdbId;
                    mov.ids = ids;
                    postData.movies = moviesList;
                    postData.movies.Add(mov);


                    listRemovalRequest.SetContent(Json.ToJson(postData));
                    HttpClient.Execute(listRemovalRequest);
                }
            }
        }
        
        public virtual NetImportPageableRequestChain GetMovies()
        {
            var pageableRequests = new NetImportPageableRequestChain();

            pageableRequests.Add(GetMovies(null));

            return pageableRequests;
        }

        private void Authenticate()
        {
            if (_configService.TraktRefreshToken != string.Empty)
            {
                //tokens were overwritten with something other than nothing
                if (_configService.NewTraktTokenExpiry > _configService.TraktTokenExpiry)
                {
                    //but our refreshedTokens are more current
                    _configService.TraktAuthToken = _configService.NewTraktAuthToken;
                    _configService.TraktRefreshToken = _configService.NewTraktRefreshToken;
                    _configService.TraktTokenExpiry = _configService.NewTraktTokenExpiry;
                }

                var unixTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

                if (unixTime > _configService.TraktTokenExpiry)
                {
                    var requestBuilder = new HttpRequestBuilder($"{RadarrTraktUrl + _configService.TraktRefreshToken}")
                    {
                        LogResponseContent = true
                    };

                    requestBuilder.Method = HttpMethod.GET;

                    var authLoginRequest = requestBuilder
                        .SetHeader("Content-Type", "application/json")
                        .Accept(HttpAccept.Json)
                        .Build();

                    var response = HttpClient.Execute(authLoginRequest);
                    var result = Json.Deserialize<RefreshRequestResponse>(response.Content);

                    _configService.TraktAuthToken = result.access_token;
                    _configService.TraktRefreshToken = result.refresh_token;

                    //lets have it expire in 8 weeks (4838400 seconds)
                    _configService.TraktTokenExpiry = unixTime + 4838400;

                    //store the refreshed tokens in case they get overwritten by an old set of tokens
                    _configService.NewTraktAuthToken = _configService.TraktAuthToken;
                    _configService.NewTraktRefreshToken = _configService.TraktRefreshToken;
                    _configService.NewTraktTokenExpiry = _configService.TraktTokenExpiry;
                }
            }
        }

        private IEnumerable<NetImportRequest> GetMovies(string searchParameters)
        {
            var link = Settings.Link.Trim();

            var filtersAndLimit = $"?years={Settings.Years}&genres={Settings.Genres.ToLower()}&ratings={Settings.Rating}&certifications={Settings.Ceritification.ToLower()}&limit={Settings.Limit}{Settings.TraktAdditionalParameters}";

            switch (Settings.ListType)
            {
                case (int)TraktListType.UserCustomList:
                    var listName = Parser.Parser.ToUrlSlug(Settings.Listname.Trim());
                    link = link + $"/users/{Settings.Username.Trim()}/lists/{listName}/items/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktListType.UserWatchList:
                    link = link + $"/users/{Settings.Username.Trim()}/watchlist/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktListType.UserWatchedList:
                    link = link + $"/users/{Settings.Username.Trim()}/watched/movies?limit={Settings.Limit}";
                    break;
                case (int)TraktListType.Trending:
                    link = link + "/movies/trending" + filtersAndLimit;
                    break;
                case (int)TraktListType.Popular:
                    link = link + "/movies/popular" + filtersAndLimit;
                    break;
                case (int)TraktListType.Anticipated:
                    link = link + "/movies/anticipated" + filtersAndLimit;
                    break;
                case (int)TraktListType.BoxOffice:
                    link = link + "/movies/boxoffice" + filtersAndLimit;
                    break;
                case (int)TraktListType.TopWatchedByWeek:
                    link = link + "/movies/watched/weekly" + filtersAndLimit;
                    break;
                case (int)TraktListType.TopWatchedByMonth:
                    link = link + "/movies/watched/monthly" + filtersAndLimit;
                    break;
                case (int)TraktListType.TopWatchedByYear:
                    link = link + "/movies/watched/yearly" + filtersAndLimit;
                    break;
                case (int)TraktListType.TopWatchedByAllTime:
                    link = link + "/movies/watched/all" + filtersAndLimit;
                    break;
            }

            Authenticate();

            var request = new NetImportRequest($"{link}", HttpAccept.Json);
            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", "964f67b126ade0112c4ae1f0aea3a8fb03190f71117bd83af6a0560a99bc52e6"); //aeon
            if (_configService.TraktAuthToken.IsNotNullOrWhiteSpace())
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + _configService.TraktAuthToken);
            }

            yield return request;
        }
    }
}
