using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Organizer;

namespace NzbDrone.Core.Music
{
    public class MoveArtistService : IExecute<MoveArtistCommand>, IExecute<BulkMoveArtistCommand>
    {
        private readonly IArtistService _artistService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskProvider _diskProvider;
        private readonly IRootFolderWatchingService _rootFolderWatchingService;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MoveArtistService(IArtistService artistService,
                                 IBuildFileNames filenameBuilder,
                                 IDiskProvider diskProvider,
                                 IRootFolderWatchingService rootFolderWatchingService,
                                 IDiskTransferService diskTransferService,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _artistService = artistService;
            _filenameBuilder = filenameBuilder;
            _diskProvider = diskProvider;
            _rootFolderWatchingService = rootFolderWatchingService;
            _diskTransferService = diskTransferService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void MoveSingleArtist(Artist artist, string sourcePath, string destinationPath, int? index = null, int? total = null)
        {
            if (!_diskProvider.FolderExists(sourcePath))
            {
                _logger.Debug("Folder '{0}' for '{1}' does not exist, not moving.", sourcePath, artist.Name);
                return;
            }

            if (index != null && total != null)
            {
                _logger.ProgressInfo("Moving {0} from '{1}' to '{2}' ({3}/{4})", artist.Name, sourcePath, destinationPath, index + 1, total);
            }
            else
            {
                _logger.ProgressInfo("Moving {0} from '{1}' to '{2}'", artist.Name, sourcePath, destinationPath);
            }

            try
            {
                _rootFolderWatchingService.ReportFileSystemChangeBeginning(sourcePath, destinationPath);

                _diskTransferService.TransferFolder(sourcePath, destinationPath, TransferMode.Move);

                _logger.ProgressInfo("{0} moved successfully to {1}", artist.Name, artist.Path);

                _eventAggregator.PublishEvent(new ArtistMovedEvent(artist, sourcePath, destinationPath));
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to move artist from '{0}' to '{1}'. Try moving files manually", sourcePath, destinationPath);

                RevertPath(artist.Id, sourcePath);
            }
        }

        private void RevertPath(int artistId, string path)
        {
            var artist = _artistService.GetArtist(artistId);

            artist.Path = path;
            _artistService.UpdateArtist(artist);
        }

        public void Execute(MoveArtistCommand message)
        {
            var artist = _artistService.GetArtist(message.ArtistId);
            MoveSingleArtist(artist, message.SourcePath, message.DestinationPath);
        }

        public void Execute(BulkMoveArtistCommand message)
        {
            var artistToMove = message.Artist;
            var destinationRootFolder = message.DestinationRootFolder;

            _logger.ProgressInfo("Moving {0} artist to '{1}'", artistToMove.Count, destinationRootFolder);

            for (var index = 0; index < artistToMove.Count; index++)
            {
                var s = artistToMove[index];
                var artist = _artistService.GetArtist(s.ArtistId);
                var destinationPath = Path.Combine(destinationRootFolder, _filenameBuilder.GetArtistFolder(artist));

                MoveSingleArtist(artist, s.SourcePath, destinationPath, index, artistToMove.Count);
            }

            _logger.ProgressInfo("Finished moving {0} artist to '{1}'", artistToMove.Count, destinationRootFolder);
        }
    }
}
