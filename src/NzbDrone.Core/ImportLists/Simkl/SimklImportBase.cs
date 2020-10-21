using System;
using System.Collections.Generic;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Notifications.Simkl;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Simkl
{
    public abstract class SimklImportBase<TSettings> : HttpImportListBase<TSettings>
    where TSettings : SimklSettingsBase<TSettings>, new()
    {
        public ISimklProxy _SimklProxy;
        private readonly IImportListRepository _importListRepository;
        public override ImportListType ListType => ImportListType.Simkl;

        protected SimklImportBase(IImportListRepository importListRepository,
                                  ISimklProxy simklProxy,
                                  IHttpClient httpClient,
                                  IImportListStatusService importListStatusService,
                                  IConfigService configService,
                                  IParsingService parsingService,
                                  Logger logger)
            : base(httpClient, importListStatusService, configService, parsingService, logger)
        {
            _importListRepository = importListRepository;
            _SimklProxy = simklProxy;
        }

        public override ImportListFetchResult Fetch()
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

        public override IParseImportListResponse GetParser()
        {
            return new SimklParser();
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = _SimklProxy.GetOAuthRequest(query["callbackUrl"]);

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
                    authUser = _SimklProxy.GetUserName(query["access_token"])
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
                var response = _SimklProxy.RefreshAuthToken(Settings.RefreshToken);

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
                _logger.Warn($"Error refreshing Simkl access token");
            }
        }
    }
}
