using System;
using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Notifications.Tdarr
{
    public class Tdarr : NotificationBase<TdarrSettings>
    {
        private readonly ITdarrProxy _proxy;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public Tdarr(ITdarrProxy proxy, ILocalizationService localizationService, Logger logger)
        {
            _proxy = proxy;
            _localizationService = localizationService;
            _logger = logger;
        }

        public override string Name => "TDarr";
        public override string Link => "https://home.tdarr.io/";

        public override void OnDownload(DownloadMessage message)
        {
            ScanFile(Path.Combine(message.Movie.Path, message.MovieFile.RelativePath));
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            foreach (var renamedFile in renamedFiles)
            {
                ScanFile(Path.Combine(movie.Path, renamedFile.MovieFile.RelativePath));
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            try
            {
                _proxy.Test(Settings);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to connect to TDarr");
                failures.Add(new ValidationFailure("Host", _localizationService.GetLocalizedString("NotificationsValidationUnableToConnectToService", new Dictionary<string, object> { { "serviceName", "TDarr" } })));
            }

            return new ValidationResult(failures);
        }

        private void ScanFile(string path)
        {
            var mappedPath = new OsPath(path);

            if (Settings.MapTo.IsNotNullOrWhiteSpace())
            {
                mappedPath = new OsPath(Settings.MapTo) + (mappedPath - new OsPath(Settings.MapFrom));
            }

            _proxy.ScanFile(mappedPath.ToString(), Settings);
        }
    }
}
