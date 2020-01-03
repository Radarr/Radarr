using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.SkyHook.Resource;
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

        protected ISpotifyProxy _spotifyProxy;
        private readonly IMetadataRequestBuilder _requestBuilder;

        protected SpotifyImportListBase(ISpotifyProxy spotifyProxy,
                                        IMetadataRequestBuilder requestBuilder,
                                        IImportListStatusService importListStatusService,
                                        IImportListRepository importListRepository,
                                        IConfigService configService,
                                        IParsingService parsingService,
                                        IHttpClient httpClient,
                                        Logger logger)
        : base(importListStatusService, configService, parsingService, logger)
        {
            _httpClient = httpClient;
            _importListRepository = importListRepository;
            _spotifyProxy = spotifyProxy;
            _requestBuilder = requestBuilder;
        }

        public override ImportListType ListType => ImportListType.Spotify;

        public string AccessToken => Settings.AccessToken;

        public void RefreshToken()
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

        public SpotifyWebAPI GetApi()
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

        public override IList<ImportListItemInfo> Fetch()
        {
            IList<SpotifyImportListItemInfo> releases;
            using (var api = GetApi())
            {
                _logger.Debug("Starting spotify import list sync");
                releases = Fetch(api);
            }

            // map to musicbrainz ids
            releases = MapSpotifyReleases(releases);

            return CleanupListItems(releases);
        }

        public abstract IList<SpotifyImportListItemInfo> Fetch(SpotifyWebAPI api);

        protected DateTime ParseSpotifyDate(string date, string precision)
        {
            if (date.IsNullOrWhiteSpace() || precision.IsNullOrWhiteSpace())
            {
                return default(DateTime);
            }

            string format;

            switch (precision)
            {
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

        public IList<SpotifyImportListItemInfo> MapSpotifyReleases(IList<SpotifyImportListItemInfo> items)
        {
            // first pass bulk lookup, server won't do search
            var spotifyIds = items.Select(x => x.ArtistSpotifyId)
                .Concat(items.Select(x => x.AlbumSpotifyId))
                .Where(x => x.IsNotNullOrWhiteSpace())
                .Distinct();
            var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                .SetSegment("route", "spotify/lookup")
                .Build();
            httpRequest.SetContent(spotifyIds.ToJson());
            httpRequest.Headers.ContentType = "application/json";

            _logger.Trace($"Requesting maps for:\n{spotifyIds.ToJson()}");

            Dictionary<string, string> map;
            try
            {
                var httpResponse = _httpClient.Post<List<SpotifyMap>>(httpRequest);
                var mapList = httpResponse.Resource;

                // Generate a mapping dictionary.
                // The API will return 0 to mean it has previously searched and can't find the item.
                // null means that it has never been searched before.
                map = mapList.Where(x => x.MusicbrainzId.IsNotNullOrWhiteSpace())
                    .ToDictionary(x => x.SpotifyId, x => x.MusicbrainzId);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                map = new Dictionary<string, string>();
            }

            _logger.Trace("Got mapping:\n{0}", map.ToJson());

            foreach (var item in items)
            {
                if (item.AlbumSpotifyId.IsNotNullOrWhiteSpace())
                {
                    if (map.ContainsKey(item.AlbumSpotifyId))
                    {
                        item.AlbumMusicBrainzId = map[item.AlbumSpotifyId];
                    }
                    else
                    {
                        MapAlbumItem(item);
                    }
                }
                else if (item.ArtistSpotifyId.IsNotNullOrWhiteSpace())
                {
                    if (map.ContainsKey(item.ArtistSpotifyId))
                    {
                        item.ArtistMusicBrainzId = map[item.ArtistSpotifyId];
                    }
                    else
                    {
                        MapArtistItem(item);
                    }
                }
            }

            // Strip out items where mapped to not found
            return items.Where(x => x.AlbumMusicBrainzId != "0" && x.ArtistMusicBrainzId != "0").ToList();
        }

        public void MapArtistItem(SpotifyImportListItemInfo item)
        {
            if (item.ArtistSpotifyId.IsNullOrWhiteSpace())
            {
                return;
            }

            var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                .SetSegment("route", $"spotify/artist/{item.ArtistSpotifyId}")
                .Build();
            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            try
            {
                var response = _httpClient.Get<ArtistResource>(httpRequest);

                if (response.HasHttpError)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        item.ArtistMusicBrainzId = "0";
                        return;
                    }
                    else
                    {
                        throw new HttpException(httpRequest, response);
                    }
                }

                item.ArtistMusicBrainzId = response.Resource.Id;
            }
            catch (HttpException e)
            {
                _logger.Warn(e, "Unable to communicate with LidarrAPI");
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        public void MapAlbumItem(SpotifyImportListItemInfo item)
        {
            if (item.AlbumSpotifyId.IsNullOrWhiteSpace())
            {
                return;
            }

            var httpRequest = _requestBuilder.GetRequestBuilder().Create()
                .SetSegment("route", $"spotify/album/{item.AlbumSpotifyId}")
                .Build();
            httpRequest.AllowAutoRedirect = true;
            httpRequest.SuppressHttpError = true;

            try
            {
                var response = _httpClient.Get<AlbumResource>(httpRequest);

                if (response.HasHttpError)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        item.AlbumMusicBrainzId = "0";
                        return;
                    }
                    else
                    {
                        throw new HttpException(httpRequest, response);
                    }
                }

                item.ArtistMusicBrainzId = response.Resource.ArtistId;
                item.AlbumMusicBrainzId = response.Resource.Id;
            }
            catch (HttpException e)
            {
                _logger.Warn(e, "Unable to communicate with LidarrAPI");
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
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
                    var profile = _spotifyProxy.GetPrivateProfile(this, api);
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
                };
            }

            return new { };
        }
    }
}
