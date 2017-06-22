using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles
{
    public interface IMoveTrackFiles
    {
        TrackFile MoveTrackFile(TrackFile trackFile, Artist artist);
        TrackFile MoveTrackFile(TrackFile trackFile, LocalTrack localTrack);
        TrackFile CopyTrackFile(TrackFile trackFile, LocalTrack localTrack);
    }

    public class TrackFileMovingService : IMoveTrackFiles
    {
        private readonly ITrackService _trackService;
        //private readonly IUpdateTrackFileService _updateTrackFileService;
        private readonly IBuildFileNames _buildFileNames;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileAttributeService _mediaFileAttributeService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public TrackFileMovingService(ITrackService episodeService,
                                //IUpdateEpisodeFileService updateEpisodeFileService,
                                IBuildFileNames buildFileNames,
                                IDiskTransferService diskTransferService,
                                IDiskProvider diskProvider,
                                IMediaFileAttributeService mediaFileAttributeService,
                                IEventAggregator eventAggregator,
                                IConfigService configService,
                                Logger logger)
        {
            _trackService = episodeService;
            //_updateTrackFileService = updateEpisodeFileService;
            _buildFileNames = buildFileNames;
            _diskTransferService = diskTransferService;
            _diskProvider = diskProvider;
            _mediaFileAttributeService = mediaFileAttributeService;
            _eventAggregator = eventAggregator;
            _configService = configService;
            _logger = logger;
        }

        public TrackFile MoveTrackFile(TrackFile trackFile, Artist artist)
        {
            throw new System.NotImplementedException();
            // TODO
            //var tracks = _trackService.GetTracksByFileId(trackFile.Id);
            //var newFileName = _buildFileNames.BuildFileName(tracks, artist, trackFile);
            //var filePath = _buildFileNames.BuildFilePath(artist, tracks.First(), trackFile.AlbumId, newFileName, Path.GetExtension(trackFile.RelativePath));

            //EnsureAlbumFolder(trackFile, artist, tracks.Select(v => v.Album).First(), filePath);

            //_logger.Debug("Renaming track file: {0} to {1}", trackFile, filePath);

            //return TransferFile(trackFile, artist, tracks, filePath, TransferMode.Move);
        }

        public TrackFile MoveTrackFile(TrackFile trackFile, LocalTrack localTrack)
        {
            // TODO
            throw new System.NotImplementedException();
            //var newFileName = _buildFileNames.BuildFileName(localEpisode.Episodes, localEpisode.Series, episodeFile);
            //var filePath = _buildFileNames.BuildFilePath(localEpisode.Series, localEpisode.SeasonNumber, newFileName, Path.GetExtension(localEpisode.Path));

            //EnsureEpisodeFolder(episodeFile, localEpisode, filePath);

            //_logger.Debug("Moving episode file: {0} to {1}", episodeFile.Path, filePath);

            //return TransferFile(episodeFile, localEpisode.Series, localEpisode.Episodes, filePath, TransferMode.Move);
        }

        public TrackFile CopyTrackFile(TrackFile trackFile, LocalTrack localTrack)
        {
            // TODO
            throw new System.NotImplementedException();
            //var newFileName = _buildFileNames.BuildFileName(localEpisode.Episodes, localEpisode.Series, episodeFile);
            //var filePath = _buildFileNames.BuildFilePath(localEpisode.Series, localEpisode.SeasonNumber, newFileName, Path.GetExtension(localEpisode.Path));

            //EnsureEpisodeFolder(episodeFile, localEpisode, filePath);

            //if (_configService.CopyUsingHardlinks)
            //{
            //    _logger.Debug("Hardlinking episode file: {0} to {1}", episodeFile.Path, filePath);
            //    return TransferFile(episodeFile, localEpisode.Series, localEpisode.Episodes, filePath, TransferMode.HardLinkOrCopy);
            //}

            //_logger.Debug("Copying episode file: {0} to {1}", episodeFile.Path, filePath);
            //return TransferFile(episodeFile, localEpisode.Series, localEpisode.Episodes, filePath, TransferMode.Copy);
        }

        private EpisodeFile TransferFile(EpisodeFile episodeFile, Series series, List<Episode> episodes, string destinationFilePath, TransferMode mode)
        {
            // TODO
            throw new System.NotImplementedException();

            //Ensure.That(episodeFile, () => episodeFile).IsNotNull();
            //Ensure.That(series, () => series).IsNotNull();
            //Ensure.That(destinationFilePath, () => destinationFilePath).IsValidPath();

            //var episodeFilePath = episodeFile.Path ?? Path.Combine(series.Path, episodeFile.RelativePath);

            //if (!_diskProvider.FileExists(episodeFilePath))
            //{
            //    throw new FileNotFoundException("Episode file path does not exist", episodeFilePath);
            //}

            //if (episodeFilePath == destinationFilePath)
            //{
            //    throw new SameFilenameException("File not moved, source and destination are the same", episodeFilePath);
            //}

            //_diskTransferService.TransferFile(episodeFilePath, destinationFilePath, mode);

            //episodeFile.RelativePath = series.Path.GetRelativePath(destinationFilePath);

            //_updateTrackFileService.ChangeFileDateForFile(episodeFile, series, episodes);

            //try
            //{
            //    _mediaFileAttributeService.SetFolderLastWriteTime(series.Path, episodeFile.DateAdded);

            //    if (series.SeasonFolder)
            //    {
            //        var seasonFolder = Path.GetDirectoryName(destinationFilePath);

            //        _mediaFileAttributeService.SetFolderLastWriteTime(seasonFolder, episodeFile.DateAdded);
            //    }
            //}

            //catch (Exception ex)
            //{
            //    _logger.Warn(ex, "Unable to set last write time");
            //}

            //_mediaFileAttributeService.SetFilePermissions(destinationFilePath);

            //return episodeFile;
        }

        private void EnsureEpisodeFolder(EpisodeFile episodeFile, LocalEpisode localEpisode, string filePath)
        {
            EnsureEpisodeFolder(episodeFile, localEpisode.Series, localEpisode.SeasonNumber, filePath);
        }

        private void EnsureEpisodeFolder(EpisodeFile episodeFile, Series series, int seasonNumber, string filePath)
        {
            var episodeFolder = Path.GetDirectoryName(filePath);
            var seasonFolder = _buildFileNames.BuildSeasonPath(series, seasonNumber);
            var seriesFolder = series.Path;
            var rootFolder = new OsPath(seriesFolder).Directory.FullPath;

            if (!_diskProvider.FolderExists(rootFolder))
            {
                throw new DirectoryNotFoundException(string.Format("Root folder '{0}' was not found.", rootFolder));
            }

            var changed = false;
            var newEvent = new EpisodeFolderCreatedEvent(series, episodeFile);

            if (!_diskProvider.FolderExists(seriesFolder))
            {
                CreateFolder(seriesFolder);
                newEvent.SeriesFolder = seriesFolder;
                changed = true;
            }

            if (seriesFolder != seasonFolder && !_diskProvider.FolderExists(seasonFolder))
            {
                CreateFolder(seasonFolder);
                newEvent.SeasonFolder = seasonFolder;
                changed = true;
            }

            if (seasonFolder != episodeFolder && !_diskProvider.FolderExists(episodeFolder))
            {
                CreateFolder(episodeFolder);
                newEvent.EpisodeFolder = episodeFolder;
                changed = true;
            }

            if (changed)
            {
                _eventAggregator.PublishEvent(newEvent);
            }
        }

        private void CreateFolder(string directoryName)
        {
            Ensure.That(directoryName, () => directoryName).IsNotNullOrWhiteSpace();

            var parentFolder = new OsPath(directoryName).Directory.FullPath;
            if (!_diskProvider.FolderExists(parentFolder))
            {
                CreateFolder(parentFolder);
            }

            try
            {
                _diskProvider.CreateFolder(directoryName);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to create directory: {0}", directoryName);
            }

            _mediaFileAttributeService.SetFolderPermissions(directoryName);
        }
    }
}
