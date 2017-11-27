using System;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDeleteMediaFiles
    {
        void DeleteTrackFile(Artist artist, TrackFile trackFile);
    }

    public class MediaFileDeletionService : IDeleteMediaFiles, IHandleAsync<ArtistDeletedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly Logger _logger;

        public MediaFileDeletionService(IDiskProvider diskProvider,
                                        IRecycleBinProvider recycleBinProvider,
                                        IMediaFileService mediaFileService,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _logger = logger;
        }

        public void DeleteTrackFile(Artist artist, TrackFile trackFile)
        {
            var fullPath = Path.Combine(artist.Path, trackFile.RelativePath);
            var rootFolder = _diskProvider.GetParentFolder(artist.Path);

            if (!_diskProvider.FolderExists(rootFolder))
            {
                _logger.Warn("Artist's root folder ({0}) doesn't exist.", rootFolder);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Artist's root folder ({0}) doesn't exist.", rootFolder);
            }

            if (_diskProvider.GetDirectories(rootFolder).Empty())
            {
                _logger.Warn("Artist's root folder ({0}) is empty.", rootFolder);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Artist's root folder ({0}) is empty.", rootFolder);
            }

            if (_diskProvider.FolderExists(artist.Path) && _diskProvider.FileExists(fullPath))
            {
                _logger.Info("Deleting track file: {0}", fullPath);

                var subfolder = _diskProvider.GetParentFolder(artist.Path).GetRelativePath(_diskProvider.GetParentFolder(fullPath));

                try
                {
                    _recycleBinProvider.DeleteFile(fullPath, subfolder);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Unable to delete track file");
                    throw new NzbDroneClientException(HttpStatusCode.InternalServerError, "Unable to delete track file");
                }
            }

            // Delete the track file from the database to clean it up even if the file was already deleted
            _mediaFileService.Delete(trackFile, DeleteMediaFileReason.Manual);
        }

        public void HandleAsync(ArtistDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                if (_diskProvider.FolderExists(message.Artist.Path))
                {
                    _recycleBinProvider.DeleteFolder(message.Artist.Path);
                }
            }
        }
    }
}
