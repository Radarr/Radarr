using System;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDeleteMediaFiles
    {
        void DeleteTrackFile(Artist artist, TrackFile trackFile);
        void DeleteTrackFile(TrackFile trackFile, string subfolder = "");
    }

    public class MediaFileDeletionService : IDeleteMediaFiles,
                                            IHandleAsync<ArtistDeletedEvent>,
                                            IHandle<TrackFileDeletedEvent>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IArtistService _artistService;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public MediaFileDeletionService(IDiskProvider diskProvider,
                                        IRecycleBinProvider recycleBinProvider,
                                        IMediaFileService mediaFileService,
                                        IArtistService artistService,
                                        IConfigService configService,
                                        Logger logger)
        {
            _diskProvider = diskProvider;
            _recycleBinProvider = recycleBinProvider;
            _mediaFileService = mediaFileService;
            _artistService = artistService;
            _configService = configService;
            _logger = logger;
        }

        public void DeleteTrackFile(Artist artist, TrackFile trackFile)
        {
            var fullPath = trackFile.Path;
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

            if (_diskProvider.FolderExists(artist.Path))
            {
                var subfolder = _diskProvider.GetParentFolder(artist.Path).GetRelativePath(_diskProvider.GetParentFolder(fullPath));
                DeleteTrackFile(trackFile, subfolder);
            }
            else
            {
                // delete from db even if the artist folder is missing
                _mediaFileService.Delete(trackFile, DeleteMediaFileReason.Manual);
            }
        }

        public void DeleteTrackFile(TrackFile trackFile, string subfolder = "")
        {
            var fullPath = trackFile.Path;

            if (_diskProvider.FileExists(fullPath))
            {
                _logger.Info("Deleting track file: {0}", fullPath);

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
                var artist = message.Artist;
                var allArtists = _artistService.GetAllArtists();

                foreach (var s in allArtists)
                {
                    if (s.Id == artist.Id) continue;

                    if (artist.Path.IsParentPath(s.Path))
                    {
                        _logger.Error("Artist path: '{0}' is a parent of another artist, not deleting files.", artist.Path);
                        return;
                    }

                    if (artist.Path.PathEquals(s.Path))
                    {
                        _logger.Error("Artist path: '{0}' is the same as another artist, not deleting files.", artist.Path);
                        return;
                    }
                }
                if (_diskProvider.FolderExists(message.Artist.Path))
                {
                    _recycleBinProvider.DeleteFolder(message.Artist.Path);
                }
            }
        }

        [EventHandleOrder(EventHandleOrder.Last)]
        public void Handle(TrackFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.Upgrade)
            {
                return;
            }

            if (_configService.DeleteEmptyFolders)
            {
                var artist = message.TrackFile.Artist.Value;
                var albumFolder = message.TrackFile.Path.GetParentPath();

                if (_diskProvider.GetFiles(artist.Path, SearchOption.AllDirectories).Empty())
                {
                    _diskProvider.DeleteFolder(artist.Path, true);
                }
                else if (_diskProvider.GetFiles(albumFolder, SearchOption.AllDirectories).Empty())
                {
                    _diskProvider.RemoveEmptySubfolders(albumFolder);
                }
            }
        }
    }
}
