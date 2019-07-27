using System;
using System.Collections.Generic;
using System.Globalization;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public abstract class SpotifyImportListBase<TSettings> : ImportListBase<TSettings>
        where TSettings : SpotifySettingsBase<TSettings>, new()
    {
        private IHttpClient _httpClient;
        private IImportListRepository _importListRepository;

        public SpotifyImportListBase(IImportListStatusService importListStatusService,
                                     IImportListRepository importListRepository,
                                     IConfigService configService,
                                     IParsingService parsingService,
                                     HttpClient httpClient,
                                     Logger logger)
        : base(importListStatusService, configService, parsingService, logger)
        {
            _httpClient = httpClient;
            _importListRepository = importListRepository;
        }

        public override ImportListType ListType => ImportListType.Spotify;

        private void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            var request = new HttpRequestBuilder(Settings.RenewUri)
                .AddQueryParam("refresh_token", Settings.RefreshToken)
                .Build();

            try
            {
                var response = _httpClient.Get<Token>(request);

                if (response != null && response.Resource != null)
                {
                    var token = response.Resource;
                    Settings.AccessToken = token.AccessToken;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                    Settings.RefreshToken = token.RefreshToken != null ? token.RefreshToken : Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _importListRepository.UpdateSettings((ImportListDefinition)Definition);
                    }
                }
            }
            catch (HttpException)
            {
                _logger.Warn($"Error refreshing spotify access token");
            }

        }
        
        protected SpotifyWebAPI GetApi()
        {
            Settings.Validate().Filter("AccessToken", "RefreshToken").ThrowOnError();
            _logger.Trace($"Access token expires at {Settings.Expires}");

            if (Settings.Expires < DateTime.UtcNow.AddMinutes(5))
            {
                RefreshToken();
            }

            return new SpotifyWebAPI
            {
                AccessToken = Settings.AccessToken,
                TokenType = "Bearer"
            };
        }

        protected T Execute<T>(SpotifyWebAPI api, Func<SpotifyWebAPI, T> method, bool allowReauth = true) where T : BasicModel
        {
            T result = method(api);
            if (result.HasError())
            {
                // If unauthorized, refresh token and try again
                if (result.Error.Status == 401)
                {
                    if (allowReauth)
                    {
                        _logger.Debug("Spotify authorization error, refreshing token and retrying");
                        RefreshToken();
                        api.AccessToken = Settings.AccessToken;
                        return Execute(api, method, false);
                    }
                    else
                    {
                        throw new SpotifyAuthorizationException(result.Error.Message);
                    }
                }
                else
                {
                    throw new SpotifyException("[{0}] {1}", result.Error.Status, result.Error.Message);
                }
            }

            return result;
        }

        public override IList<ImportListItemInfo> Fetch()
        {
            using (var api = GetApi())
            {
                _logger.Debug("Starting spotify import list sync");
                var releases = Fetch(api);
                return CleanupListItems(releases);
            }
        }

        public abstract IList<ImportListItemInfo> Fetch(SpotifyWebAPI api);

        protected DateTime ParseSpotifyDate(string date, string precision)
        {
            if (date.IsNullOrWhiteSpace() || precision.IsNullOrWhiteSpace())
            {
                return default(DateTime);
            }
            
            string format;
            
            switch (precision) {
                case "year":
                    format = "yyyy";
                    break;
                case "month":
                    format = "yyyy-MM";
                    break;
                case "day":
                default:
                    format = "yyyy-MM-dd";
                    break;
            }

            return DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result) ? result : default(DateTime);
        }

        protected override void Test(List<ValidationFailure> failures)
        {
            failures.AddIfNotNull(TestConnection());
        }

        private ValidationFailure TestConnection()
        {
            try
            {
                using (var api = GetApi())
                {
                    var profile = Execute(api, (x) => x.GetPrivateProfile());
                    _logger.Debug($"Connected to spotify profile {profile.DisplayName} [{profile.Id}]");
                    return null;
                }
            }
            catch (SpotifyAuthorizationException ex)
            {
                _logger.Warn(ex, "Spotify Authentication Error");
                return new ValidationFailure(string.Empty, $"Spotify authentication error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to connect to Spotify");

                return new ValidationFailure(string.Empty, "Unable to connect to import list, check the log for more details");
            }
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = new HttpRequestBuilder(Settings.OAuthUrl)
                    .AddQueryParam("client_id", Settings.ClientId)
                    .AddQueryParam("response_type", "code")
                    .AddQueryParam("redirect_uri", Settings.RedirectUri)
                    .AddQueryParam("scope", Settings.Scope)
                    .AddQueryParam("state", query["callbackUrl"])
                    .AddQueryParam("show_dialog", true)
                    .Build();

                return new {
                    OauthUrl = request.Url.ToString()
                };
            }
            else if (action == "getOAuthToken")
            {
                return new {
                    accessToken = query["access_token"],
                    expires = DateTime.UtcNow.AddSeconds(int.Parse(query["expires_in"])),
                    refreshToken = query["refresh_token"],
                };
            }

            return new { };
        }
    }
}
