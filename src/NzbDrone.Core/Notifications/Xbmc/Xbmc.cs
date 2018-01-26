﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications.Xbmc
{
    public class Xbmc : NotificationBase<XbmcSettings>
    {
        private readonly IXbmcService _xbmcService;
        private readonly Logger _logger;

        public Xbmc(IXbmcService xbmcService, Logger logger)
        {
            _xbmcService = xbmcService;
            _logger = logger;
        }

        public override string Link => "http://xbmc.org/";

        public override void OnGrab(GrabMessage grabMessage)
        {
            const string header = "Radarr - Grabbed";

            Notify(Settings, header, grabMessage.Message);
        }

        public override void OnDownload(DownloadMessage message)
        {
            const string header = "Radarr - Downloaded";

            Notify(Settings, header, message.Message);
            UpdateAndCleanMovie(message.Movie, message.OldMovieFiles.Any());
        }

        public override void OnMovieRename(Movie movie)
        {
            UpdateAndCleanMovie(movie);
        }

        public override string Name => "Kodi (XBMC)";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_xbmcService.Test(Settings, "Success! XBMC has been successfully configured!"));

            return new ValidationResult(failures);
        }

        private void Notify(XbmcSettings settings, string header, string message)
        {
            try
            {
                if (Settings.Notify)
                {
                    _xbmcService.Notify(Settings, header, message);
                }
            }
            catch (SocketException ex)
            {
                var logMessage = string.Format("Unable to connect to XBMC Host: {0}:{1}", Settings.Host, Settings.Port);
                _logger.Debug(ex, logMessage);
            }
        }

        private void UpdateAndCleanMovie(Movie movie, bool clean = true)
        {
            try
            {
                if (Settings.UpdateLibrary)
                {
                    _xbmcService.UpdateMovie(Settings, movie);
                }

                if (clean && Settings.CleanLibrary)
                {
                    _xbmcService.Clean(Settings);
                }
            }
            catch (SocketException ex)
            {
                var logMessage = string.Format("Unable to connect to XBMC Host: {0}:{1}", Settings.Host, Settings.Port);
                _logger.Debug(ex, logMessage);
            }
        }
    }
}
