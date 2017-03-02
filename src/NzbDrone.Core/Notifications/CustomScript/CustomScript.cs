using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Processes;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Music;
using NzbDrone.Core.ThingiProvider;
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

        public override string Name => "Custom Script";

        public override string Link => "https://github.com/Lidarr/Lidarr/wiki/Custom-Post-Processing-Scripts";

        public override ProviderMessage Message => new ProviderMessage("Testing will execute the script with the EventType set to Test, ensure your script handles this correctly", ProviderMessageType.Warning);

        public override void OnGrab(GrabMessage message)
        {
            var artist = message.Artist;
            var remoteAlbum = message.Album;
            var releaseGroup = remoteAlbum.ParsedAlbumInfo.ReleaseGroup;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Lidarr_EventType", "Grab");
            environmentVariables.Add("Lidarr_Artist_Id", artist.Id.ToString());
            environmentVariables.Add("Lidarr_Artist_Name", artist.Metadata.Value.Name);
            environmentVariables.Add("Lidarr_Artist_MBId", artist.Metadata.Value.ForeignArtistId);
            environmentVariables.Add("Lidarr_Artist_Type", artist.Metadata.Value.Type);
            environmentVariables.Add("Lidarr_Release_AlbumCount", remoteAlbum.Albums.Count.ToString());
            environmentVariables.Add("Lidarr_Release_AlbumReleaseDates", string.Join(",", remoteAlbum.Albums.Select(e => e.ReleaseDate)));
            environmentVariables.Add("Lidarr_Release_AlbumTitles", string.Join("|", remoteAlbum.Albums.Select(e => e.Title)));
            environmentVariables.Add("Lidarr_Release_Title", remoteAlbum.Release.Title);
            environmentVariables.Add("Lidarr_Release_Indexer", remoteAlbum.Release.Indexer ?? string.Empty);
            environmentVariables.Add("Lidarr_Release_Size", remoteAlbum.Release.Size.ToString());
            environmentVariables.Add("Lidarr_Release_Quality", remoteAlbum.ParsedAlbumInfo.Quality.Quality.Name);
            environmentVariables.Add("Lidarr_Release_QualityVersion", remoteAlbum.ParsedAlbumInfo.Quality.Revision.Version.ToString());
            environmentVariables.Add("Lidarr_Release_ReleaseGroup", releaseGroup ?? string.Empty);
            environmentVariables.Add("Lidarr_Download_Client", message.DownloadClient ?? string.Empty);
            environmentVariables.Add("Lidarr_Download_Id", message.DownloadId ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnReleaseImport(AlbumDownloadMessage message)
        {
            var artist = message.Artist;
            var album = message.Album;
            var release = message.Release;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Lidarr_EventType", "AlbumDownload");
            environmentVariables.Add("Lidarr_Artist_Id", artist.Id.ToString());
            environmentVariables.Add("Lidarr_Artist_Name", artist.Metadata.Value.Name);
            environmentVariables.Add("Lidarr_Artist_Path", artist.Path);
            environmentVariables.Add("Lidarr_Artist_MBId", artist.Metadata.Value.ForeignArtistId);
            environmentVariables.Add("Lidarr_Artist_Type", artist.Metadata.Value.Type);
            environmentVariables.Add("Lidarr_Album_Id", album.Id.ToString());
            environmentVariables.Add("Lidarr_Album_Title", album.Title);
            environmentVariables.Add("Lidarr_Album_MBId", album.ForeignAlbumId);
            environmentVariables.Add("Lidarr_AlbumRelease_MBId", release.ForeignReleaseId);
            environmentVariables.Add("Lidarr_Album_ReleaseDate", album.ReleaseDate.ToString());
            environmentVariables.Add("Lidarr_Download_Client", message.DownloadClient ?? string.Empty);
            environmentVariables.Add("Lidarr_Download_Id", message.DownloadId ?? string.Empty);

            if (message.TrackFiles.Any())
            {
                environmentVariables.Add("Lidarr_AddedTrackPaths", string.Join("|", message.TrackFiles.Select(e => e.Path)));
            }

            if (message.OldFiles.Any())
            {
                environmentVariables.Add("Lidarr_DeletedPaths", string.Join("|", message.OldFiles.Select(e => e.Path)));
            }

            ExecuteScript(environmentVariables);
        }

        public override void OnRename(Artist artist)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Lidarr_EventType", "Rename");
            environmentVariables.Add("Lidarr_Artist_Id", artist.Id.ToString());
            environmentVariables.Add("Lidarr_Artist_Name", artist.Metadata.Value.Name);
            environmentVariables.Add("Lidarr_Artist_Path", artist.Path);
            environmentVariables.Add("Lidarr_Artist_MBId", artist.Metadata.Value.ForeignArtistId);
            environmentVariables.Add("Lidarr_Artist_Type", artist.Metadata.Value.Type);

            ExecuteScript(environmentVariables);
        }

        public override void OnTrackRetag(TrackRetagMessage message)
        {
            var artist = message.Artist;
            var album = message.Album;
            var release = message.Release;
            var trackFile = message.TrackFile;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Lidarr_EventType", "TrackRetag");
            environmentVariables.Add("Lidarr_Artist_Id", artist.Id.ToString());
            environmentVariables.Add("Lidarr_Artist_Name", artist.Metadata.Value.Name);
            environmentVariables.Add("Lidarr_Artist_Path", artist.Path);
            environmentVariables.Add("Lidarr_Artist_MBId", artist.Metadata.Value.ForeignArtistId);
            environmentVariables.Add("Lidarr_Artist_Type", artist.Metadata.Value.Type);
            environmentVariables.Add("Lidarr_Album_Id", album.Id.ToString());
            environmentVariables.Add("Lidarr_Album_Title", album.Title);
            environmentVariables.Add("Lidarr_Album_MBId", album.ForeignAlbumId);
            environmentVariables.Add("Lidarr_AlbumRelease_MBId", release.ForeignReleaseId);
            environmentVariables.Add("Lidarr_Album_ReleaseDate", album.ReleaseDate.ToString());
            environmentVariables.Add("Lidarr_TrackFile_Id", trackFile.Id.ToString());
            environmentVariables.Add("Lidarr_TrackFile_TrackCount", trackFile.Tracks.Value.Count.ToString());
            environmentVariables.Add("Lidarr_TrackFile_Path", trackFile.Path);
            environmentVariables.Add("Lidarr_TrackFile_TrackNumbers", string.Join(",", trackFile.Tracks.Value.Select(e => e.TrackNumber)));
            environmentVariables.Add("Lidarr_TrackFile_TrackTitles", string.Join("|", trackFile.Tracks.Value.Select(e => e.Title)));
            environmentVariables.Add("Lidarr_TrackFile_Quality", trackFile.Quality.Quality.Name);
            environmentVariables.Add("Lidarr_TrackFile_QualityVersion", trackFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Lidarr_TrackFile_ReleaseGroup", trackFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Lidarr_TrackFile_SceneName", trackFile.SceneName ?? string.Empty);
            environmentVariables.Add("Lidarr_Tags_Diff", message.Diff.ToJson());
            environmentVariables.Add("Lidarr_Tags_Scrubbed", message.Scrubbed.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Lidarr_EventType", "HealthIssue");
            environmentVariables.Add("Lidarr_Health_Issue_Level", nameof(healthCheck.Type));
            environmentVariables.Add("Lidarr_Health_Issue_Message", healthCheck.Message);
            environmentVariables.Add("Lidarr_Health_Issue_Type", healthCheck.Source.Name);
            environmentVariables.Add("Lidarr_Health_Issue_Wiki", healthCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            if (!_diskProvider.FileExists(Settings.Path))
            {
                failures.Add(new NzbDroneValidationFailure("Path", "File does not exist"));
            }

            try
            {
                var environmentVariables = new StringDictionary();
                environmentVariables.Add("Lidarr_EventType", "Test");

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

            return new ValidationResult(failures);
        }

        private ProcessOutput ExecuteScript(StringDictionary environmentVariables)
        {
            _logger.Debug("Executing external script: {0}", Settings.Path);

            var processOutput = _processProvider.StartAndCapture(Settings.Path, Settings.Arguments, environmentVariables);

            _logger.Debug("Executed external script: {0} - Status: {1}", Settings.Path, processOutput.ExitCode);
            _logger.Debug($"Script Output: {System.Environment.NewLine}{string.Join(System.Environment.NewLine, processOutput.Lines)}");

            return processOutput;
        }
    }
}
