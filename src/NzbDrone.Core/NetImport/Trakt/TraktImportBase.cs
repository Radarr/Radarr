using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Trakt
{
    public abstract class TraktImportBase<TSettings> : HttpNetImportBase<TSettings>
    where TSettings : TraktSettingsBase<TSettings>, new()
    {
        public override NetImportType ListType => NetImportType.Trakt;

        public const string OAuthUrl = "https://api.trakt.tv/oauth/authorize";
        public const string RedirectUri = "https://auth.servarr.com/v1/trakt/auth";
        public const string RenewUri = "https://auth.servarr.com/v1/trakt/renew";
        public const string ClientId = "64508a8bf370cee550dde4806469922fd7cd70afb2d5690e3ee7f75ae784b70e";

        private INetImportRepository _netImportRepository;

        protected TraktImportBase(INetImportRepository netImportRepository,
                           IHttpClient httpClient,
                           INetImportStatusService netImportStatusService,
                           IConfigService configService,
                           IParsingService parsingService,
                           Logger logger)
            : base(httpClient, netImportStatusService, configService, parsingService, logger)
        {
            _netImportRepository = netImportRepository;
        }

        public override NetImportFetchResult Fetch()
        {
            Settings.Validate().Filter("AccessToken", "RefreshToken").ThrowOnError();
            _logger.Trace($"Access token expires at {Settings.Expires}");

            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }

            var generator = GetRequestGenerator();
            return FetchMovies(generator.GetMovies());
        }

        public override IParseNetImportResponse GetParser()
        {
            return new TraktParser();
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = new HttpRequestBuilder(OAuthUrl)
                    .AddQueryParam("client_id", ClientId)
                    .AddQueryParam("response_type", "code")
                    .AddQueryParam("redirect_uri", RedirectUri)
                    .AddQueryParam("state", query["callbackUrl"])
                    .Build();

                return new
                {
                    OauthUrl = request.Url.ToString()
                };
            }
            else if (action == "getOAuthToken")
            {
                return new
                {
                    accessToken = query["access_token"],
                    expires = DateTime.UtcNow.AddSeconds(int.Parse(query["expires_in"])),
                    refreshToken = query["refresh_token"],
                    authUser = GetUserName(query["access_token"])
                };
            }

            return new { };
        }

        private string GetUserName(string accessToken)
        {
            var request = new HttpRequestBuilder(string.Format("{0}/users/settings", Settings.Link))
                .Build();

            request.Headers.Add("trakt-api-version", "2");
            request.Headers.Add("trakt-api-key", ClientId);

            if (accessToken.IsNotNullOrWhiteSpace())
            {
                request.Headers.Add("Authorization", "Bearer " + accessToken);
            }

            try
            {
                var response = _httpClient.Get<UserSettingsResponse>(request);

                if (response != null && response.Resource != null)
                {
                    return response.Resource.User.Ids.Slug;
                }
            }
            catch (HttpException)
            {
                _logger.Warn($"Error refreshing trakt access token");
            }

            return null;
        }

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            var request = new HttpRequestBuilder(RenewUri)
                .AddQueryParam("refresh_token", Settings.RefreshToken)
                .Build();

            try
            {
                var response = _httpClient.Get<RefreshRequestResponse>(request);

                if (response != null && response.Resource != null)
                {
                    var token = response.Resource;
                    Settings.AccessToken = token.Access_token;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.Expires_in);
                    Settings.RefreshToken = token.Refresh_token != null ? token.Refresh_token : Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _netImportRepository.UpdateSettings((NetImportDefinition)Definition);
                    }
                }
            }
            catch (HttpException)
            {
                _logger.Warn($"Error refreshing trakt access token");
            }
        }
    }
}
