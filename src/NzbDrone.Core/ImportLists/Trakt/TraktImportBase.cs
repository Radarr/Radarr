using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Trakt;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Trakt
{
    public abstract class TraktImportBase<TSettings> : HttpImportListBase<TSettings>
    where TSettings : TraktSettingsBase<TSettings>, new()
    {
        public ITraktProxy _traktProxy;
        private readonly IImportListRepository _importListRepository;
        public override ImportListType ListType => ImportListType.Trakt;
        public override TimeSpan MinRefreshInterval => TimeSpan.FromHours(12);

        protected TraktImportBase(IImportListRepository importListRepository,
                                  ITraktProxy traktProxy,
                                  IHttpClient httpClient,
                                  IImportListStatusService importListStatusService,
                                  IConfigService configService,
                                  IParsingService parsingService,
                                  Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _importListRepository = importListRepository;
            _traktProxy = traktProxy;
        }

        public override ImportListFetchResult Fetch()
        {
            Settings.Validate().Filter("AccessToken", "RefreshToken").ThrowOnError();
            _logger.Trace($"Access token expires at {Settings.Expires}");

            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }

            return FetchMovies(g => g.GetMovies());
        }

        public override IParseImportListResponse GetParser()
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
                        _importListRepository.UpdateSettings((ImportListDefinition)Definition);
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
