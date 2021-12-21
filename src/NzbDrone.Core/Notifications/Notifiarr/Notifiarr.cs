using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.HealthCheck;

namespace NzbDrone.Core.Notifications.Notifiarr
{
    public class Notifiarr : NotificationBase<NotifiarrSettings>
    {
        private readonly INotifiarrProxy _proxy;

        public Notifiarr(INotifiarrProxy proxy)
        {
            _proxy = proxy;
        }

        public override string Link => "https://notifiarr.com";
        public override string Name => "Notifiarr";

        public override void OnGrab(GrabMessage message)
        {
            var movie = message.Movie;
            var remoteMovie = message.RemoteMovie;
            var quality = message.Quality;
            var variables = new StringDictionary();

            variables.Add("Radarr_EventType", "Grab");
            variables.Add("Radarr_Movie_Id", movie.Id.ToString());
            variables.Add("Radarr_Movie_Title", movie.Title);
            variables.Add("Radarr_Movie_Year", movie.Year.ToString());
            variables.Add("Radarr_Movie_ImdbId", movie.ImdbId ?? string.Empty);
            variables.Add("Radarr_Movie_TmdbId", movie.TmdbId.ToString());
            variables.Add("Radarr_Movie_In_Cinemas_Date", movie.InCinemas.ToString() ?? string.Empty);
            variables.Add("Radarr_Movie_Physical_Release_Date", movie.PhysicalRelease.ToString() ?? string.Empty);
            variables.Add("Radarr_Release_Title", remoteMovie.Release.Title);
            variables.Add("Radarr_Release_Indexer", remoteMovie.Release.Indexer ?? string.Empty);
            variables.Add("Radarr_Release_Size", remoteMovie.Release.Size.ToString());
            variables.Add("Radarr_Release_ReleaseGroup", remoteMovie.ParsedMovieInfo.ReleaseGroup ?? string.Empty);
            variables.Add("Radarr_Release_Quality", quality.Quality.Name);
            variables.Add("Radarr_Release_QualityVersion", quality.Revision.Version.ToString());
            variables.Add("Radarr_IndexerFlags", remoteMovie.Release.IndexerFlags.ToString());
            variables.Add("Radarr_Download_Client", message.DownloadClient ?? string.Empty);
            variables.Add("Radarr_Download_Id", message.DownloadId ?? string.Empty);

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var movie = message.Movie;
            var movieFile = message.MovieFile;
            var sourcePath = message.SourcePath;
            var variables = new StringDictionary();

            variables.Add("Radarr_EventType", "Download");
            variables.Add("Radarr_IsUpgrade", message.OldMovieFiles.Any().ToString());
            variables.Add("Radarr_Movie_Id", movie.Id.ToString());
            variables.Add("Radarr_Movie_Title", movie.Title);
            variables.Add("Radarr_Movie_Year", movie.Year.ToString());
            variables.Add("Radarr_Movie_Path", movie.Path);
            variables.Add("Radarr_Movie_ImdbId", movie.ImdbId ?? string.Empty);
            variables.Add("Radarr_Movie_TmdbId", movie.TmdbId.ToString());
            variables.Add("Radarr_Movie_In_Cinemas_Date", movie.InCinemas.ToString() ?? string.Empty);
            variables.Add("Radarr_Movie_Physical_Release_Date", movie.PhysicalRelease.ToString() ?? string.Empty);
            variables.Add("Radarr_MovieFile_Id", movieFile.Id.ToString());
            variables.Add("Radarr_MovieFile_RelativePath", movieFile.RelativePath);
            variables.Add("Radarr_MovieFile_Path", Path.Combine(movie.Path, movieFile.RelativePath));
            variables.Add("Radarr_MovieFile_Quality", movieFile.Quality.Quality.Name);
            variables.Add("Radarr_MovieFile_QualityVersion", movieFile.Quality.Revision.Version.ToString());
            variables.Add("Radarr_MovieFile_ReleaseGroup", movieFile.ReleaseGroup ?? string.Empty);
            variables.Add("Radarr_MovieFile_SceneName", movieFile.SceneName ?? string.Empty);
            variables.Add("Radarr_MovieFile_SourcePath", sourcePath);
            variables.Add("Radarr_MovieFile_SourceFolder", Path.GetDirectoryName(sourcePath));
            variables.Add("Radarr_Download_Id", message.DownloadId ?? string.Empty);
            variables.Add("Radarr_Download_Client", message.DownloadClient ?? string.Empty);

            if (message.OldMovieFiles.Any())
            {
                variables.Add("Radarr_DeletedRelativePaths", string.Join("|", message.OldMovieFiles.Select(e => e.RelativePath)));
                variables.Add("Radarr_DeletedPaths", string.Join("|", message.OldMovieFiles.Select(e => Path.Combine(movie.Path, e.RelativePath))));
            }

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            var movie = deleteMessage.Movie;
            var movieFile = deleteMessage.MovieFile;

            var variables = new StringDictionary();

            variables.Add("Radarr_EventType", "MovieFileDelete");
            variables.Add("Radarr_MovieFile_DeleteReason", deleteMessage.Reason.ToString());
            variables.Add("Radarr_Movie_Id", movie.Id.ToString());
            variables.Add("Radarr_Movie_Title", movie.Title);
            variables.Add("Radarr_Movie_Year", movie.Year.ToString());
            variables.Add("Radarr_Movie_Path", movie.Path);
            variables.Add("Radarr_Movie_ImdbId", movie.ImdbId ?? string.Empty);
            variables.Add("Radarr_Movie_TmdbId", movie.TmdbId.ToString());
            variables.Add("Radarr_MovieFile_Id", movieFile.Id.ToString());
            variables.Add("Radarr_MovieFile_RelativePath", movieFile.RelativePath);
            variables.Add("Radarr_MovieFile_Path", Path.Combine(movie.Path, movieFile.RelativePath));
            variables.Add("Radarr_MovieFile_Size", movieFile.Size.ToString());
            variables.Add("Radarr_MovieFile_Quality", movieFile.Quality.Quality.Name);
            variables.Add("Radarr_MovieFile_QualityVersion", movieFile.Quality.Revision.Version.ToString());
            variables.Add("Radarr_MovieFile_ReleaseGroup", movieFile.ReleaseGroup ?? string.Empty);
            variables.Add("Radarr_MovieFile_SceneName", movieFile.SceneName ?? string.Empty);

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            var movie = deleteMessage.Movie;
            var variables = new StringDictionary();

            variables.Add("Radarr_EventType", "MovieDelete");
            variables.Add("Radarr_Movie_Id", movie.Id.ToString());
            variables.Add("Radarr_Movie_Title", movie.Title);
            variables.Add("Radarr_Movie_Year", movie.Year.ToString());
            variables.Add("Radarr_Movie_Path", movie.Path);
            variables.Add("Radarr_Movie_ImdbId", movie.ImdbId ?? string.Empty);
            variables.Add("Radarr_Movie_TmdbId", movie.TmdbId.ToString());
            variables.Add("Radarr_Movie_DeletedFiles", deleteMessage.DeletedFiles.ToString());
            if (deleteMessage.DeletedFiles && movie.MovieFile != null)
            {
                variables.Add("Radarr_Movie_Folder_Size", movie.MovieFile.Size.ToString());
            }

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var variables = new StringDictionary();

            variables.Add("Radarr_EventType", "HealthIssue");
            variables.Add("Radarr_Health_Issue_Level", Enum.GetName(typeof(HealthCheckResult), healthCheck.Type));
            variables.Add("Radarr_Health_Issue_Message", healthCheck.Message);
            variables.Add("Radarr_Health_Issue_Type", healthCheck.Source.Name);
            variables.Add("Radarr_Health_Issue_Wiki", healthCheck.WikiUrl.ToString() ?? string.Empty);

            _proxy.SendNotification(variables, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var variables = new StringDictionary();

            variables.Add("Radarr_EventType", "ApplicationUpdate");
            variables.Add("Radarr_Update_Message", updateMessage.Message);
            variables.Add("Radarr_Update_NewVersion", updateMessage.NewVersion.ToString());
            variables.Add("Radarr_Update_PreviousVersion", updateMessage.PreviousVersion.ToString());

            _proxy.SendNotification(variables, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(_proxy.Test(Settings));

            return new ValidationResult(failures);
        }
    }
}
