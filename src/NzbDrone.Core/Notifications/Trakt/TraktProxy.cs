using System;
using System.Net.Http;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Trakt.Resource;

namespace NzbDrone.Core.Notifications.Trakt
{
    public interface ITraktProxy
    {
        string GetUserName(string accessToken);
        HttpRequest GetOAuthRequest(string callbackUrl);
        TraktAuthRefreshResource RefreshAuthToken(string refreshToken);
        void AddToCollection(TraktCollectMoviesResource payload, string accessToken);
        void RemoveFromCollection(TraktCollectMoviesResource payload, string accessToken);
        HttpRequest BuildRequest(string resource, HttpMethod method, string accessToken);
    }

    public class TraktProxy : ITraktProxy
    {
        private const string URL = "https://api.trakt.tv";
        private const string OAuthUrl = "https://trakt.tv/oauth/authorize";
        private const string RedirectUri = "https://auth.servarr.com/v1/trakt/auth";
        private const string RenewUri = "https://auth.servarr.com/v1/trakt/renew";
        private const string ClientId = "64508a8bf370cee550dde4806469922fd7cd70afb2d5690e3ee7f75ae784b70e";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public TraktProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void AddToCollection(TraktCollectMoviesResource payload, string accessToken)
        {
            var request = BuildRequest("sync/collection", HttpMethod.Post, accessToken);

            request.Headers.ContentType = "application/json";
            request.SetContent(payload.ToJson());

            MakeRequest(request);
        }

        public void RemoveFromCollection(TraktCollectMoviesResource payload, string accessToken)
        {
            var request = BuildRequest("sync/collection/remove", HttpMethod.Post, accessToken);

            request.Headers.ContentType = "application/json";
            request.SetContent(payload.ToJson());

            MakeRequest(request);
        }

        public string GetUserName(string accessToken)
        {
            var request = BuildRequest("users/settings", HttpMethod.Get, accessToken);
            var response = _httpClient.Get<TraktUserSettingsResource>(request);

            return response?.Resource?.User?.Ids?.Slug;
        }

        public HttpRequest GetOAuthRequest(string callbackUrl)
        {
            return new HttpRequestBuilder(OAuthUrl)
                            .AddQueryParam("client_id", ClientId)
                            .AddQueryParam("response_type", "code")
                            .AddQueryParam("redirect_uri", RedirectUri)
                            .AddQueryParam("state", callbackUrl)
                            .Build();
        }

        public TraktAuthRefreshResource RefreshAuthToken(string refreshToken)
        {
            var request = new HttpRequestBuilder(RenewUri)
                    .AddQueryParam("refresh_token", refreshToken)
                    .WithRateLimit(2)
                    .Build();

            return _httpClient.Get<TraktAuthRefreshResource>(request)?.Resource ?? null;
        }

        public HttpRequest BuildRequest(string resource, HttpMethod method, string accessToken)
        {
            var request = new HttpRequestBuilder(URL).Resource(resource).Build();

            request.RateLimit = TimeSpan.FromSeconds(2);
            request.Headers.Accept = HttpAccept.Json.Value;
            request.Method = method;

            request.Headers.Add("trakt-api-version", "2");
            request.Headers.Add("trakt-api-key", ClientId);

            if (accessToken.IsNotNullOrWhiteSpace())
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            return request;
        }

        private void MakeRequest(HttpRequest request)
        {
            try
            {
                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                throw new TraktException("Unable to send payload", ex);
            }
        }
    }
}
