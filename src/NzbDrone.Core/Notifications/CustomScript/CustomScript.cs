using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.CustomScript
{
    public class CustomScript : NotificationBase<CustomScriptSettings>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public CustomScript(IDiskProvider diskProvider, IProcessProvider processProvider, Logger logger)
        {
            _diskProvider = diskProvider;
            _processProvider = processProvider;
            _logger = logger;
        }

        public override string Link => "https://github.com/Radarr/Radarr/wiki/Custom-Post-Processing-Scripts";

        public override void OnGrab(GrabMessage message)
        {
            var movie = message.Movie;
            var remoteMovie = message.RemoteMovie;
            var quality = message.Quality;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "Grab");
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.Title);
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.ImdbId);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.TmdbId.ToString());
            environmentVariables.Add("Radarr_Release_Title", remoteMovie.Release.Title);
            environmentVariables.Add("Radarr_Release_Indexer", remoteMovie.Release.Indexer);
            environmentVariables.Add("Radarr_Release_Size", remoteMovie.Release.Size.ToString());
            environmentVariables.Add("Radarr_Release_ReleaseGroup", remoteMovie.ParsedMovieInfo.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Radarr_Release_Quality", quality.Quality.Name);
            environmentVariables.Add("Radarr_Release_QualityVersion", quality.Revision.Version.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var movie = message.Movie;
            var movieFile = message.MovieFile;
            var sourcePath = message.SourcePath;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "Download");
            environmentVariables.Add("Radarr_IsUpgrade", message.OldMovieFiles.Any().ToString());
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.Title);
            environmentVariables.Add("Radarr_Movie_Path", movie.Path);
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.ImdbId);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.TmdbId.ToString());
            environmentVariables.Add("Radarr_MovieFile_Id", movieFile.Id.ToString());
            environmentVariables.Add("Radarr_MovieFile_RelativePath", movieFile.RelativePath);
            environmentVariables.Add("Radarr_MovieFile_Path", Path.Combine(movie.Path, movieFile.RelativePath));
            environmentVariables.Add("Radarr_MovieFile_Quality", movieFile.Quality.Quality.Name);
            environmentVariables.Add("Radarr_MovieFile_QualityVersion", movieFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Radarr_MovieFile_ReleaseGroup", movieFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Radarr_MovieFile_SceneName", movieFile.SceneName ?? string.Empty);
            environmentVariables.Add("Radarr_MovieFile_SourcePath", sourcePath);
            environmentVariables.Add("Radarr_MovieFile_SourceFolder", Path.GetDirectoryName(sourcePath));
            environmentVariables.Add("Radarr_Download_Id", message.DownloadId ?? string.Empty);

            if (message.OldMovieFiles.Any())
            {
                environmentVariables.Add("Radarr_DeletedRelativePaths", string.Join("|", message.OldMovieFiles.Select(e => e.RelativePath)));
                environmentVariables.Add("Radarr_DeletedPaths", string.Join("|", message.OldMovieFiles.Select(e => Path.Combine(movie.Path, e.RelativePath))));
            }
            ExecuteScript(environmentVariables);
        }

        public override void OnMovieRename(Movie movie)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "Rename");
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.Title);
            environmentVariables.Add("Radarr_Movie_Path", movie.Path);
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.ImdbId);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.TmdbId.ToString());

            ExecuteScript(environmentVariables);
        }

        public override string Name => "Custom Script";

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            if (!_diskProvider.FileExists(Settings.Path))
            {
                failures.Add(new NzbDroneValidationFailure("Path", "File does not exist"));
            }

            return new ValidationResult(failures);
        }

        private void ExecuteScript(StringDictionary environmentVariables)
        {
            _logger.Debug("Executing external script: {0}", Settings.Path);

            var process = _processProvider.StartAndCapture(Settings.Path, Settings.Arguments, environmentVariables);

            _logger.Debug("Executed external script: {0} - Status: {1}", Settings.Path, process.ExitCode);
            _logger.Debug("Script Output: \r\n{0}", string.Join("\r\n", process.Lines));
        }
    }
}
