using System.IO;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Music
{
    public class MoveArtistService : IExecute<MoveArtistCommand>
    {
        private readonly IArtistService _artistService;
        private readonly IBuildFileNames _filenameBuilder;
        private readonly IDiskTransferService _diskTransferService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public MoveArtistService(IArtistService artistService,
                                 IBuildFileNames filenameBuilder,
                                 IDiskTransferService diskTransferService,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _artistService = artistService;
            _filenameBuilder = filenameBuilder;
            _diskTransferService = diskTransferService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void Execute(MoveArtistCommand message)
        {
            var artist = _artistService.GetArtist(message.ArtistId);
            var source = message.SourcePath;
            var destination = message.DestinationPath;

            if (!message.DestinationRootFolder.IsNullOrWhiteSpace())
            {
                _logger.Debug("Buiding destination path using root folder: {0} and the artist name", message.DestinationRootFolder);
                destination = Path.Combine(message.DestinationRootFolder, _filenameBuilder.GetArtistFolder(artist));
            }

            _logger.ProgressInfo("Moving {0} from '{1}' to '{2}'", artist.Name, source, destination);

            //TODO: Move to transactional disk operations
            try
            {
                _diskTransferService.TransferFolder(source, destination, TransferMode.Move);
            }
            catch (IOException ex)
            {
                _logger.Error(ex, "Unable to move artist from '{0}' to '{1}'", source, destination);
                throw;
            }

            _logger.ProgressInfo("{0} moved successfully to {1}", artist.Name, artist.Path);

            //Update the artist path to the new path
            artist.Path = destination;
            artist = _artistService.UpdateArtist(artist);

            _eventAggregator.PublishEvent(new ArtistMovedEvent(artist, source, destination));
        }
    }
}
