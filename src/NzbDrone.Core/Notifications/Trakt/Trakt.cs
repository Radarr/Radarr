using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Trakt
{
    public class Trakt : NotificationBase<TraktSettings>
    {
        private readonly ITraktProxy _proxy;
        private readonly INotificationRepository _notificationRepository;
        private readonly Logger _logger;

        public Trakt(ITraktProxy proxy, INotificationRepository notificationRepository, Logger logger)
        {
            _proxy = proxy;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public override string Link => "https://trakt.tv/";
        public override string Name => "Trakt";

        public override void OnDownload(DownloadMessage message)
        {
            var payload = new TraktAddMoviePayload
            {
                Movies = new List<TraktAddMovie>()
            };

            var width = message.MovieFile.MediaInfo?.Width ?? 0;
            var mediaInfo = "";

            if (width >= 3200)
            {
                mediaInfo = "uhd_4k";
            }

            if (width >= 1800)
            {
                mediaInfo = "hd_1080p";
            }

            if (width >= 1200)
            {
                mediaInfo = "hd_720p";
            }

            if (width >= 700)
            {
                mediaInfo = "sd_576p";
            }

            if (width > 0)
            {
                mediaInfo = "sd_480p";
            }

            payload.Movies.Add(new TraktAddMovie
            {
                Title = message.Movie.Title,
                Year = message.Movie.Year,
                CollectedAt = DateTime.Now,
                Resolution = mediaInfo,
                Ids = new TraktMovieIdsResource
                {
                    Tmdb = message.Movie.TmdbId,
                    Imdb = message.Movie.ImdbId ?? "",
                }
            });

            _proxy.AddToCollection(payload, Settings.AccessToken);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = _proxy.GetOAuthRequest(query["callbackUrl"]);

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
                    authUser = _proxy.GetUserName(query["access_token"])
                };
            }

            return new { };
        }

        public void RefreshToken()
        {
            _logger.Trace("Refreshing Token");

            Settings.Validate().Filter("RefreshToken").ThrowOnError();

            try
            {
                var response = _proxy.RefreshAuthToken(Settings.RefreshToken);

                if (response != null)
                {
                    var token = response;
                    Settings.AccessToken = token.Access_token;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.Expires_in);
                    Settings.RefreshToken = token.Refresh_token != null ? token.Refresh_token : Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _notificationRepository.UpdateSettings((NotificationDefinition)Definition);
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
