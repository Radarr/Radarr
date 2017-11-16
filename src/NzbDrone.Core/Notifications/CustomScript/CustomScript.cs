using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Processes;
using NzbDrone.Core.Music;
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

        public override void OnGrab(GrabMessage message)
        {
            var artist = message.Artist;
            var remoteAlbum = message.Album;
            var releaseGroup = remoteAlbum.ParsedAlbumInfo.ReleaseGroup;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Lidarr_EventType", "Grab");
            environmentVariables.Add("Lidarr_Artist_Id", artist.Id.ToString());
            environmentVariables.Add("Lidarr_Artist_Name", artist.Name);
            environmentVariables.Add("Lidarr_Artist_MBId", artist.ForeignArtistId.ToString());
            environmentVariables.Add("Lidarr_Artist_Type", artist.ArtistType);
            environmentVariables.Add("Lidarr_Release_AlbumCount", remoteAlbum.Albums.Count.ToString());
            environmentVariables.Add("Lidarr_Release_AlbumReleaseDates", string.Join(",", remoteAlbum.Albums.Select(e => e.ReleaseDate)));
            environmentVariables.Add("Lidarr_Release_AlbumTitles", string.Join("|", remoteAlbum.Albums.Select(e => e.Title)));
            environmentVariables.Add("Lidarr_Release_Title", remoteAlbum.Release.Title);
            environmentVariables.Add("Lidarr_Release_Indexer", remoteAlbum.Release.Indexer);
            environmentVariables.Add("Lidarr_Release_Size", remoteAlbum.Release.Size.ToString());
            environmentVariables.Add("Lidarr_Release_Quality", remoteAlbum.ParsedAlbumInfo.Quality.Quality.Name);
            environmentVariables.Add("Lidarr_Release_QualityVersion", remoteAlbum.ParsedAlbumInfo.Quality.Revision.Version.ToString());
            environmentVariables.Add("Lidarr_Release_ReleaseGroup", releaseGroup ?? string.Empty);
            environmentVariables.Add("Lidarr_Download_Client", message.DownloadClient ?? string.Empty);
            environmentVariables.Add("Lidarr_Download_Id", message.DownloadId ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var artist = message.Artist;
            var trackFile = message.TrackFile;
            var sourcePath = message.SourcePath;
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Lidarr_EventType", "Download");
            environmentVariables.Add("Lidarr_IsUpgrade", message.OldFiles.Any().ToString());
            environmentVariables.Add("Lidarr_Artist_Id", artist.Id.ToString());
            environmentVariables.Add("Lidarr_Artist_Name", artist.Name);
            environmentVariables.Add("Lidarr_Artist_Path", artist.Path);
            environmentVariables.Add("Lidarr_Artist_MBId", artist.ForeignArtistId.ToString());
            environmentVariables.Add("Lidarr_Artist_Type", artist.ArtistType);
            environmentVariables.Add("Lidarr_TrackFile_Id", trackFile.Id.ToString());
            environmentVariables.Add("Lidarr_TrackFile_TrackCount", trackFile.Tracks.Value.Count.ToString());
            environmentVariables.Add("Lidarr_TrackFile_RelativePath", trackFile.RelativePath);
            environmentVariables.Add("Lidarr_TrackFile_Path", Path.Combine(artist.Path, trackFile.RelativePath));
            environmentVariables.Add("Lidarr_TrackFile_TrackNumbers", string.Join(",", trackFile.Tracks.Value.Select(e => e.TrackNumber)));
            environmentVariables.Add("Lidarr_TrackFile_TrackReleaseDates", string.Join(",", trackFile.Tracks.Value.Select(e => e.Album.ReleaseDate)));
            environmentVariables.Add("Lidarr_TrackFile_TrackTitles", string.Join("|", trackFile.Tracks.Value.Select(e => e.Title)));
            environmentVariables.Add("Lidarr_TrackFile_Quality", trackFile.Quality.Quality.Name);
            environmentVariables.Add("Lidarr_TrackFile_QualityVersion", trackFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Lidarr_TrackFile_ReleaseGroup", trackFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Lidarr_TrackFile_SceneName", trackFile.SceneName ?? string.Empty);
            environmentVariables.Add("Lidarr_TrackFile_SourcePath", sourcePath);
            environmentVariables.Add("Lidarr_TrackFile_SourceFolder", Path.GetDirectoryName(sourcePath));
            environmentVariables.Add("Lidarr_Download_Client", message.DownloadClient ?? string.Empty);
            environmentVariables.Add("Lidarr_Download_Id", message.DownloadId ?? string.Empty);

            if (message.OldFiles.Any())
            {
                environmentVariables.Add("Lidarr_DeletedRelativePaths", string.Join("|", message.OldFiles.Select(e => e.RelativePath)));
                environmentVariables.Add("Lidarr_DeletedPaths", string.Join("|", message.OldFiles.Select(e => Path.Combine(artist.Path, e.RelativePath))));
            }

            ExecuteScript(environmentVariables);
        }

        public override void OnRename(Artist artist)
        {
            var environmentVariables = new StringDictionary();

            environmentVariables.Add("Lidarr_EventType", "Rename");
            environmentVariables.Add("Lidarr_Artist_Id", artist.Id.ToString());
            environmentVariables.Add("Lidarr_Artist_Name", artist.Name);
            environmentVariables.Add("Lidarr_Artist_Path", artist.Path);
            environmentVariables.Add("Lidarr_Artist_MBId", artist.ForeignArtistId.ToString());
            environmentVariables.Add("Lidarr_Artist_Type", artist.ArtistType);

            ExecuteScript(environmentVariables);
        }


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
