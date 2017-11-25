using NLog;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public class ArtistScannedHandler : IHandle<ArtistScannedEvent>,
                                        IHandle<ArtistScanSkippedEvent>
    {
        private readonly IAlbumMonitoredService _albumMonitoredService;
        private readonly IArtistService _artistService;
        private readonly IManageCommandQueue _commandQueueManager;
        //private readonly IEpisodeAddedService _episodeAddedService;

        private readonly Logger _logger;

        public ArtistScannedHandler(IAlbumMonitoredService albumMonitoredService,
                                    IArtistService artistService,
                                    IManageCommandQueue commandQueueManager,
                                    //IEpisodeAddedService episodeAddedService,
                                    Logger logger)
        {
            _albumMonitoredService = albumMonitoredService;
            _artistService = artistService;
            _commandQueueManager = commandQueueManager;
            //_episodeAddedService = episodeAddedService;
            _logger = logger;
        }

        private void HandleScanEvents(Artist artist)
        {
            if (artist.AddOptions == null)
            {
                //_episodeAddedService.SearchForRecentlyAdded(series.Id);
                return;
            }

            _logger.Info("[{0}] was recently added, performing post-add actions", artist.Name);
            _albumMonitoredService.SetAlbumMonitoredStatus(artist, artist.AddOptions);

            if (artist.AddOptions.SearchForMissingAlbums)
            {
                _commandQueueManager.Push(new MissingAlbumSearchCommand(artist.Id));
            }

            artist.AddOptions = null;
            _artistService.RemoveAddOptions(artist);
        }

        public void Handle(ArtistScannedEvent message)
        {
            HandleScanEvents(message.Artist);
        }

        public void Handle(ArtistScanSkippedEvent message)
        {
            HandleScanEvents(message.Artist);
        }
    }
}
