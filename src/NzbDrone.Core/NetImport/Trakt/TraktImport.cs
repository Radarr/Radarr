using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;
using System;
using System.Collections.Generic;

namespace NzbDrone.Core.NetImport.Trakt
{
    public class TraktImport : HttpNetImportBase<TraktSettings>
    {
        public override string Name => "Trakt List";
        public override bool Enabled => true;
        public override bool EnableAuto => false;

        private INetImportRepository _netImportRepository;

        public TraktImport(INetImportRepository netImportRepository,
                           IHttpClient httpClient,
                           IConfigService configService,
                           IParsingService parsingService,
                           Logger logger)
            : base(httpClient, configService, parsingService, logger)
        {
            _netImportRepository = netImportRepository;
        }

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            var request = new HttpRequestBuilder(Settings.RenewUri)
                .AddQueryParam("refresh", Settings.RefreshToken)
                .Build();

            try
            {
                var response = _httpClient.Get<RefreshRequestResponse>(request);

                if (response != null && response.Resource != null)
                {
                    var token = response.Resource;
                    Settings.AccessToken = token.access_token;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.expires_in);
                    Settings.RefreshToken = token.refresh_token != null ? token.refresh_token : Settings.RefreshToken;

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

        public override INetImportRequestGenerator GetRequestGenerator()
        {
            Settings.Validate().Filter("AccessToken", "RefreshToken").ThrowOnError();
            _logger.Trace($"Access token expires at {Settings.Expires}");

            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }

            return new TraktRequestGenerator() { Settings = Settings, _configService=_configService, HttpClient = _httpClient, };
        }

        public override IParseNetImportResponse GetParser()
        {
            return new TraktParser(Settings);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = new HttpRequestBuilder(Settings.OAuthUrl)
                    .AddQueryParam("target", query["callbackUrl"])
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
                    accessToken = query["access"],
                    expires = DateTime.UtcNow.AddSeconds(4838400),
                    refreshToken = query["refresh"],
                };
            }

            return new { };
        }
    }
}
