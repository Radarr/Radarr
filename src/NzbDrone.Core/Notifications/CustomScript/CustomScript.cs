using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.CustomScript
{
    public class CustomScript : NotificationBase<CustomScriptSettings>
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public CustomScript(IConfigFileProvider configFileProvider,
            IConfigService configService,
            IDiskProvider diskProvider,
            IProcessProvider processProvider,
            Logger logger)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _diskProvider = diskProvider;
            _processProvider = processProvider;
            _logger = logger;
        }

        public override string Name => "Custom Script";

        public override string Link => "https://wiki.servarr.com/radarr/settings#connections";

        public override ProviderMessage Message => new ProviderMessage("Testing will execute the script with the EventType set to Test, ensure your script handles this correctly", ProviderMessageType.Warning);

        public override void OnGrab(GrabMessage message)
        {
            var movie = message.Movie;
            var remoteMovie = message.RemoteMovie;
            var quality = message.Quality;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "Grab");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.MovieMetadata.Value.Title);
            environmentVariables.Add("Radarr_Movie_Year", movie.MovieMetadata.Value.Year.ToString());
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.MovieMetadata.Value.ImdbId ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.MovieMetadata.Value.TmdbId.ToString());
            environmentVariables.Add("Radarr_Movie_In_Cinemas_Date", movie.MovieMetadata.Value.InCinemas.ToString() ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_Physical_Release_Date", movie.MovieMetadata.Value.PhysicalRelease.ToString() ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_Overview", movie.MovieMetadata.Value.Overview);
            environmentVariables.Add("Radarr_Release_Title", remoteMovie.Release.Title);
            environmentVariables.Add("Radarr_Release_Indexer", remoteMovie.Release.Indexer ?? string.Empty);
            environmentVariables.Add("Radarr_Release_Size", remoteMovie.Release.Size.ToString());
            environmentVariables.Add("Radarr_Release_ReleaseGroup", remoteMovie.ParsedMovieInfo.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Radarr_Release_Quality", quality.Quality.Name);
            environmentVariables.Add("Radarr_Release_QualityVersion", quality.Revision.Version.ToString());
            environmentVariables.Add("Radarr_IndexerFlags", remoteMovie.Release.IndexerFlags.ToString());
            environmentVariables.Add("Radarr_Download_Client", message.DownloadClientName ?? string.Empty);
            environmentVariables.Add("Radarr_Download_Client_Type", message.DownloadClientType ?? string.Empty);
            environmentVariables.Add("Radarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Radarr_Release_CustomFormat", string.Join("|", remoteMovie.CustomFormats));
            environmentVariables.Add("Radarr_Release_CustomFormatScore", remoteMovie.CustomFormatScore.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var movie = message.Movie;
            var movieFile = message.MovieFile;
            var sourcePath = message.SourcePath;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "Download");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_IsUpgrade", message.OldMovieFiles.Any().ToString());
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.MovieMetadata.Value.Title);
            environmentVariables.Add("Radarr_Movie_Year", movie.MovieMetadata.Value.Year.ToString());
            environmentVariables.Add("Radarr_Movie_Path", movie.Path);
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.MovieMetadata.Value.ImdbId ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.MovieMetadata.Value.TmdbId.ToString());
            environmentVariables.Add("Radarr_Movie_In_Cinemas_Date", movie.MovieMetadata.Value.InCinemas.ToString() ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_Physical_Release_Date", movie.MovieMetadata.Value.PhysicalRelease.ToString() ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_Overview", movie.MovieMetadata.Value.Overview);
            environmentVariables.Add("Radarr_MovieFile_Id", movieFile.Id.ToString());
            environmentVariables.Add("Radarr_MovieFile_RelativePath", movieFile.RelativePath);
            environmentVariables.Add("Radarr_MovieFile_Path", Path.Combine(movie.Path, movieFile.RelativePath));
            environmentVariables.Add("Radarr_MovieFile_Quality", movieFile.Quality.Quality.Name);
            environmentVariables.Add("Radarr_MovieFile_QualityVersion", movieFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Radarr_MovieFile_ReleaseGroup", movieFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Radarr_MovieFile_SceneName", movieFile.SceneName ?? string.Empty);
            environmentVariables.Add("Radarr_MovieFile_SourcePath", sourcePath);
            environmentVariables.Add("Radarr_MovieFile_SourceFolder", Path.GetDirectoryName(sourcePath));
            environmentVariables.Add("Radarr_Download_Client", message.DownloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Radarr_Download_Client_Type", message.DownloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Radarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_AudioChannels", MediaInfoFormatter.FormatAudioChannels(movieFile.MediaInfo).ToString());
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_AudioCodec", MediaInfoFormatter.FormatAudioCodec(movieFile.MediaInfo, null));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_AudioLanguages", movieFile.MediaInfo.AudioLanguages.Distinct().ConcatToString(" / "));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_Languages", movieFile.MediaInfo.AudioLanguages.ConcatToString(" / "));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_Height", movieFile.MediaInfo.Height.ToString());
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_Width", movieFile.MediaInfo.Width.ToString());
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_Subtitles", movieFile.MediaInfo.Subtitles.ConcatToString(" / "));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_VideoCodec", MediaInfoFormatter.FormatVideoCodec(movieFile.MediaInfo, null));
            environmentVariables.Add("Radarr_MovieFile_MediaInfo_VideoDynamicRangeType", MediaInfoFormatter.FormatVideoDynamicRangeType(movieFile.MediaInfo));
            environmentVariables.Add("Radarr_MovieFile_CustomFormat", string.Join("|", message.MovieInfo.CustomFormats));
            environmentVariables.Add("Radarr_MovieFile_CustomFormatScore", message.MovieInfo.CustomFormatScore.ToString());
            environmentVariables.Add("Radarr_Release_Indexer", message.Release?.Indexer);
            environmentVariables.Add("Radarr_Release_Size", message.Release?.Size.ToString());
            environmentVariables.Add("Radarr_Release_Title", message.Release?.Title);

            if (message.OldMovieFiles.Any())
            {
                environmentVariables.Add("Radarr_DeletedRelativePaths", string.Join("|", message.OldMovieFiles.Select(e => e.RelativePath)));
                environmentVariables.Add("Radarr_DeletedPaths", string.Join("|", message.OldMovieFiles.Select(e => Path.Combine(movie.Path, e.RelativePath))));
                environmentVariables.Add("Radarr_DeletedDateAdded", string.Join("|", message.OldMovieFiles.Select(e => e.DateAdded)));
            }

            ExecuteScript(environmentVariables);
        }

        public override void OnMovieRename(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "Rename");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.MovieMetadata.Value.Title);
            environmentVariables.Add("Radarr_Movie_Year", movie.MovieMetadata.Value.Year.ToString());
            environmentVariables.Add("Radarr_Movie_Path", movie.Path);
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.MovieMetadata.Value.ImdbId ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.MovieMetadata.Value.TmdbId.ToString());
            environmentVariables.Add("Radarr_Movie_In_Cinemas_Date", movie.MovieMetadata.Value.InCinemas.ToString() ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_Physical_Release_Date", movie.MovieMetadata.Value.PhysicalRelease.ToString() ?? string.Empty);
            environmentVariables.Add("Radarr_MovieFile_Ids", string.Join(",", renamedFiles.Select(e => e.MovieFile.Id)));
            environmentVariables.Add("Radarr_MovieFile_RelativePaths", string.Join("|", renamedFiles.Select(e => e.MovieFile.RelativePath)));
            environmentVariables.Add("Radarr_MovieFile_Paths", string.Join("|", renamedFiles.Select(e => e.MovieFile.Path)));
            environmentVariables.Add("Radarr_MovieFile_PreviousRelativePaths", string.Join("|", renamedFiles.Select(e => e.PreviousRelativePath)));
            environmentVariables.Add("Radarr_MovieFile_PreviousPaths", string.Join("|", renamedFiles.Select(e => e.PreviousPath)));

            ExecuteScript(environmentVariables);
        }

        public override void OnMovieAdded(Movie movie)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "MovieAdded");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.MovieMetadata.Value.Title);
            environmentVariables.Add("Radarr_Movie_Year", movie.MovieMetadata.Value.Year.ToString());
            environmentVariables.Add("Radarr_Movie_Path", movie.Path);
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.MovieMetadata.Value.ImdbId ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.MovieMetadata.Value.TmdbId.ToString());
            environmentVariables.Add("Radarr_Movie_AddMethod", movie.AddOptions.AddMethod.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            var movie = deleteMessage.Movie;
            var movieFile = deleteMessage.MovieFile;

            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "MovieFileDelete");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_MovieFile_DeleteReason", deleteMessage.Reason.ToString());
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.Title);
            environmentVariables.Add("Radarr_Movie_Year", movie.Year.ToString());
            environmentVariables.Add("Radarr_Movie_Path", movie.Path);
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.MovieMetadata.Value.ImdbId ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.MovieMetadata.Value.TmdbId.ToString());
            environmentVariables.Add("Radarr_Movie_Overview", movie.MovieMetadata.Value.Overview);
            environmentVariables.Add("Radarr_MovieFile_Id", movieFile.Id.ToString());
            environmentVariables.Add("Radarr_MovieFile_RelativePath", movieFile.RelativePath);
            environmentVariables.Add("Radarr_MovieFile_Path", Path.Combine(movie.Path, movieFile.RelativePath));
            environmentVariables.Add("Radarr_MovieFile_Size", movieFile.Size.ToString());
            environmentVariables.Add("Radarr_MovieFile_Quality", movieFile.Quality.Quality.Name);
            environmentVariables.Add("Radarr_MovieFile_QualityVersion", movieFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Radarr_MovieFile_ReleaseGroup", movieFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Radarr_MovieFile_SceneName", movieFile.SceneName ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            var movie = deleteMessage.Movie;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "MovieDelete");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.MovieMetadata.Value.Title);
            environmentVariables.Add("Radarr_Movie_Year", movie.MovieMetadata.Value.Year.ToString());
            environmentVariables.Add("Radarr_Movie_Path", movie.Path);
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.MovieMetadata.Value.ImdbId ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.MovieMetadata.Value.TmdbId.ToString());
            environmentVariables.Add("Radarr_Movie_DeletedFiles", deleteMessage.DeletedFiles.ToString());
            environmentVariables.Add("Radarr_Movie_Overview", movie.MovieMetadata.Value.Overview);

            if (deleteMessage.DeletedFiles && movie.MovieFile != null)
            {
                environmentVariables.Add("Radarr_Movie_Folder_Size", movie.MovieFile.Size.ToString());
            }

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "HealthIssue");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_Health_Issue_Level", Enum.GetName(typeof(HealthCheckResult), healthCheck.Type));
            environmentVariables.Add("Radarr_Health_Issue_Message", healthCheck.Message);
            environmentVariables.Add("Radarr_Health_Issue_Type", healthCheck.Source.Name);
            environmentVariables.Add("Radarr_Health_Issue_Wiki", healthCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "HealthRestored");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_Health_Restored_Level", Enum.GetName(typeof(HealthCheckResult), previousCheck.Type));
            environmentVariables.Add("Radarr_Health_Restored_Message", previousCheck.Message);
            environmentVariables.Add("Radarr_Health_Restored_Type", previousCheck.Source.Name);
            environmentVariables.Add("Radarr_Health_Restored_Wiki", previousCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "ApplicationUpdate");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_Update_Message", updateMessage.Message);
            environmentVariables.Add("Radarr_Update_NewVersion", updateMessage.NewVersion.ToString());
            environmentVariables.Add("Radarr_Update_PreviousVersion", updateMessage.PreviousVersion.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var movie = message.Movie;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Radarr_EventType", "ManualInteractionRequired");
            environmentVariables.Add("Radarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Radarr_ApplicationUrl", _configService.ApplicationUrl);
            environmentVariables.Add("Radarr_Movie_Id", movie.Id.ToString());
            environmentVariables.Add("Radarr_Movie_Title", movie.MovieMetadata.Value.Title);
            environmentVariables.Add("Radarr_Movie_Year", movie.MovieMetadata.Value.Year.ToString());
            environmentVariables.Add("Radarr_Movie_Path", movie.Path);
            environmentVariables.Add("Radarr_Movie_ImdbId", movie.MovieMetadata.Value.ImdbId ?? string.Empty);
            environmentVariables.Add("Radarr_Movie_TmdbId", movie.MovieMetadata.Value.TmdbId.ToString());
            environmentVariables.Add("Radarr_Movie_Overview", movie.MovieMetadata.Value.Overview);
            environmentVariables.Add("Radarr_Download_Client", message.DownloadClientName ?? string.Empty);
            environmentVariables.Add("Radarr_Download_Client_Type", message.DownloadClientType ?? string.Empty);
            environmentVariables.Add("Radarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Radarr_Download_Size", message.TrackedDownload.DownloadItem.TotalSize.ToString());
            environmentVariables.Add("Radarr_Download_Title", message.TrackedDownload.DownloadItem.Title);

            ExecuteScript(environmentVariables);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            if (!_diskProvider.FileExists(Settings.Path))
            {
                failures.Add(new NzbDroneValidationFailure("Path", "File does not exist"));
            }

            foreach (var systemFolder in SystemFolders.GetSystemFolders())
            {
                if (systemFolder.IsParentPath(Settings.Path))
                {
                    failures.Add(new NzbDroneValidationFailure("Path", $"Must not be a descendant of '{systemFolder}'"));
                }
            }

            if (failures.Empty())
            {
                try
                {
                    var environmentVariables = new StringDictionary
                    {
                        { "Radarr_EventType", "Test" },
                        { "Radarr_InstanceName", _configFileProvider.InstanceName },
                        { "Radarr_ApplicationUrl", _configService.ApplicationUrl }
                    };

                    var processOutput = ExecuteScript(environmentVariables);

                    if (processOutput.ExitCode != 0)
                    {
                        failures.Add(new NzbDroneValidationFailure(string.Empty, $"Script exited with code: {processOutput.ExitCode}"));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    failures.Add(new NzbDroneValidationFailure(string.Empty, ex.Message));
                }
            }

            return new ValidationResult(failures);
        }

        private ProcessOutput ExecuteScript(StringDictionary environmentVariables)
        {
            _logger.Debug("Executing external script: {0}", Settings.Path);

            var processOutput = _processProvider.StartAndCapture(Settings.Path, Settings.Arguments, environmentVariables);

            _logger.Debug("Executed external script: {0} - Status: {1}", Settings.Path, processOutput.ExitCode);
            _logger.Debug("Script Output: \r\n{0}", string.Join("\r\n", processOutput.Lines));

            return processOutput;
        }

        private bool ValidatePathParent(string possibleParent, string path)
        {
            return possibleParent.IsParentPath(path);
        }
    }
}
