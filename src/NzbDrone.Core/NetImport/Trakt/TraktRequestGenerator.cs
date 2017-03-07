﻿using NzbDrone.Common.Http;
using System;
using System.Collections.Generic;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;


namespace NzbDrone.Core.NetImport.Trakt
{
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

            var filters = $"?years={Settings.Years}&genres={Settings.Genres.ToLower()}&ratings={Settings.Rating}&certifications={Settings.Ceritification.ToLower()}";

            switch (Settings.ListType)
            {
                case (int)TraktListType.UserCustomList:
                    link = link + $"/users/{Settings.Username.Trim()}/lists/{Settings.Listname.Trim()}/items/movies";
                    break;
                case (int)TraktListType.UserWatchList:
                    link = link + $"/users/{Settings.Username.Trim()}/watchlist/movies";
                    break;
                case (int)TraktListType.UserWatchedList:
                    link = link + $"/users/{Settings.Username.Trim()}/watched/movies";
                    break;
                case (int)TraktListType.Trending:
                    link = link + "/movies/trending" + filters;
                    break;
                case (int)TraktListType.Popular:
                    link = link + "/movies/popular" + filters;
                    break;
                case (int)TraktListType.Anticipated:
                    link = link + "/movies/anticipated" + filters;
                    break;
                case (int)TraktListType.BoxOffice:
                    link = link + "/movies/boxoffice" + filters;
                    break;
                case (int)TraktListType.TopWatchedByWeek:
                    link = link + "/movies/watched/weekly" + filters;
                    break;
                case (int)TraktListType.TopWatchedByMonth:
                    link = link + "/movies/watched/monthly" + filters;
                    break;
                case (int)TraktListType.TopWatchedByYear:
                    link = link + "/movies/watched/yearly" + filters;
                    break;
                case (int)TraktListType.TopWatchedByAllTime:
                    link = link + "/movies/watched/all" + filters;
                    break;
            }

            Authenticate();            

            var request = new NetImportRequest($"{link}", HttpAccept.Json);
            request.HttpRequest.Headers.Add("trakt-api-version", "2");
            request.HttpRequest.Headers.Add("trakt-api-key", "964f67b126ade0112c4ae1f0aea3a8fb03190f71117bd83af6a0560a99bc52e6"); //aeon
            if (_configService.TraktAuthToken != null)
            {
                request.HttpRequest.Headers.Add("Authorization", "Bearer " + _configService.TraktAuthToken);
            }

            yield return request;
        }
    }
}
