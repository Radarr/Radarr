using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Trakt
{
    public abstract class TraktImportBase<TSettings> : HttpNetImportBase<TSettings>
    where TSettings : TraktSettingsBase<TSettings>, new()
    {
        public ITraktProxy _traktProxy;
        private readonly INetImportRepository _netImportRepository;
        public override NetImportType ListType => NetImportType.Trakt;

        protected TraktImportBase(INetImportRepository netImportRepository,
                           ITraktProxy traktProxy,
                           IHttpClient httpClient,
                           INetImportStatusService netImportStatusService,
                           IConfigService configService,
                           IParsingService parsingService,
                           Logger logger)
            : base(httpClient, netImportStatusService, configService, parsingService, logger)
        {
            _netImportRepository = netImportRepository;
            _traktProxy = traktProxy;
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
                var request = _traktProxy.GetOAuthRequest(query["callbackUrl"]);

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
                    authUser = _traktProxy.GetUserName(query["access_token"])
                };
            }

            return new { };
        }

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            try
            {
                var response = _traktProxy.RefreshAuthToken(Settings.RefreshToken);

                if (response != null)
                {
                    var token = response;
                    Settings.AccessToken = token.AccessToken;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                    Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

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
