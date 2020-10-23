using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Simkl.Resource;

namespace NzbDrone.Core.Notifications.Simkl
{
    public interface ISimklProxy
    {
        string GetUserName(string accessToken);
        HttpRequest GetOAuthRequest(string callbackUrl);
        SimklAuthRefreshResource RefreshAuthToken(string refreshToken);
        void AddToCollection(SimklCollectMoviesResource payload, string accessToken);
        void RemoveFromCollection(SimklCollectMoviesResource payload, string accessToken);
        HttpRequest BuildSimklRequest(string resource, HttpMethod method, string accessToken);
    }

    public class SimklProxy : ISimklProxy
    {
        private const string URL = "https://simkl.com";
        private const string OAuthUrl = "https://simkl.com/oauth/authorize";
        private const string RedirectUri = "https://auth.servarr.com/v1/simkl/auth";
        private const string RenewUri = "https://auth.servarr.com/v1/simkl/renew";
        private const string ClientId = "03d807fd74d79caa123c19dc7a2f1cb5851604e6b0e2530b03986fc95c4f19c6";

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public SimklProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void AddToCollection(SimklCollectMoviesResource payload, string accessToken)
        {
            var request = BuildSimklRequest("sync/collection", HttpMethod.POST, accessToken);

            request.Headers.ContentType = "application/json";
            request.SetContent(payload.ToJson());

            try
            {
                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, "Unable to post payload {0}", payload);
                throw new SimklException("Unable to post payload", ex);
            }
        }

        public void RemoveFromCollection(SimklCollectMoviesResource payload, string accessToken)
        {
            var request = BuildSimklRequest("sync/collection/remove", HttpMethod.POST, accessToken);

            request.Headers.ContentType = "application/json";
            request.SetContent(payload.ToJson());

            try
            {
                _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                _logger.Error(ex, "Unable to post payload {0}", payload);
                throw new SimklException("Unable to post payload", ex);
            }
        }

        public string GetUserName(string accessToken)
        {
            var request = BuildSimklRequest("users/settings", HttpMethod.GET, accessToken);

            try
            {
                var response = _httpClient.Get<SimklUserSettingsResource>(request);

                if (response != null && response.Resource != null)
                {
                    return response.Resource.User.Ids.Slug;
                }
            }
            catch (HttpException)
            {
                _logger.Warn($"Error refreshing Simkl access token");
            }

            return null;
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

        public SimklAuthRefreshResource RefreshAuthToken(string refreshToken)
        {
            var request = new HttpRequestBuilder(RenewUri)
                    .AddQueryParam("refresh_token", refreshToken)
                    .Build();

            return _httpClient.Get<SimklAuthRefreshResource>(request)?.Resource ?? null;
        }

        public HttpRequest BuildSimklRequest(string resource, HttpMethod method, string accessToken)
        {
            var request = new HttpRequestBuilder(URL).Resource(resource).Build();

            request.Headers.Accept = HttpAccept.Json.Value;
            request.Method = method;

            request.Headers.Add("simkl-api-version", "2");
            request.Headers.Add("simkl-api-key", ClientId);

            if (accessToken.IsNotNullOrWhiteSpace())
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            return request;
        }
    }
}
