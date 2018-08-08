using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Music
{
    public class MoveArtistService : IExecute<MoveArtistCommand>, IExecute<BulkMoveArtistCommand>
    {
        private readonly IArtistService _artistService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MoveArtistService(IArtistService artistService,
                                 IBuildFileNames filenameBuilder,
                                 IDiskProvider diskProvider,
                                 IDiskTransferService diskTransferService,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _artistService = artistService;
            _filenameBuilder = filenameBuilder;
            _diskProvider = diskProvider;
            _diskTransferService = diskTransferService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        private void MoveSingleArtist(Artist artist, string sourcePath, string destinationPath)
        {
            if (!_diskProvider.FolderExists(sourcePath))
            {
                _logger.Debug("Folder '{0}' for '{1}' does not exist, not moving.", sourcePath, artist.Name);
                return;
            }

            _logger.ProgressInfo("Moving {0} from '{1}' to '{2}'", artist.Name, sourcePath, destinationPath);

            try
            {
                _diskTransferService.TransferFolder(sourcePath, destinationPath, TransferMode.Move);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to move artist from '{0}' to '{1}'. Try moving files manually", sourcePath, destinationPath);

                RevertPath(artist.Id, sourcePath);
            }

            _logger.ProgressInfo("{0} moved successfully to {1}", artist.Name, artist.Path);

            _eventAggregator.PublishEvent(new ArtistMovedEvent(artist, sourcePath, destinationPath));
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

            foreach (var s in artistToMove)
            {
                var artist = _artistService.GetArtist(s.ArtistId);
                var destinationPath = Path.Combine(destinationRootFolder, _filenameBuilder.GetArtistFolder(artist));

                MoveSingleArtist(artist, s.SourcePath, destinationPath);
            }

            _logger.ProgressInfo("Finished moving {0} artist to '{1}'", artistToMove.Count, destinationRootFolder);
        }
    }
}
