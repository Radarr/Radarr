using System;
using System.Collections.Generic;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Simkl
{
    public class Simkl : NotificationBase<SimklSettings>
    {
        private readonly ISimklService _simklService;
        private readonly INotificationRepository _notificationRepository;
        private readonly Logger _logger;

        public Simkl(ISimklService simklService, INotificationRepository notificationRepository, Logger logger)
        {
            _simklService = simklService;
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public override string Link => "https://simkl.com/";
        public override string Name => "Simkl";

        public override void OnDownload(DownloadMessage message)
        {
            _simklService.AddMovieToCollection(Settings, message.Movie, message.MovieFile);
        }

        public override void OnDelete(DeleteMessage message)
        {
            if (message.Reason != MediaFiles.DeleteMediaFileReason.Upgrade)
            {
                _simklService.RemoveMovieFromCollection(Settings, message.Movie, message.MovieFile);
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_simklService.Test(Settings));

            return new ValidationResult(failures);
        }

        public override object RequestAction(string action, IDictionary<string, string> query)
        {
            if (action == "startOAuth")
            {
                var request = _simklService.GetOAuthRequest(query["callbackUrl"]);

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
                    authUser = _simklService.GetUserName(query["access_token"])
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
                var response = _simklService.RefreshAuthToken(Settings.RefreshToken);

                if (response != null)
                {
                    var token = response;
                    Settings.AccessToken = token.AccessToken;
                    Settings.Expires = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                    Settings.RefreshToken = token.RefreshToken ?? Settings.RefreshToken;

                    if (Definition.Id > 0)
                    {
                        _notificationRepository.UpdateSettings((NotificationDefinition)Definition);
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
