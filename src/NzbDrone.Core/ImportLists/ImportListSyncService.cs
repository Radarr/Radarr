using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListSyncService : IExecute<ImportListSyncCommand>
    {
        private readonly IImportListStatusService _importListStatusService;
        private readonly IImportListFactory _importListFactory;
        private readonly IFetchAndParseImportList _listFetcherAndParser;
        private readonly ISearchForNewAlbum _albumSearchService;
        private readonly ISearchForNewArtist _artistSearchService;
        private readonly IArtistService _artistService;
        private readonly IAddArtistService _addArtistService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportListSyncService(IImportListStatusService importListStatusService,
                              IImportListFactory importListFactory,
                              IFetchAndParseImportList listFetcherAndParser,
                              ISearchForNewAlbum albumSearchService,
                              ISearchForNewArtist artistSearchService,
                              IArtistService artistService,
                              IAddArtistService addArtistService,
                              IEventAggregator eventAggregator,
                              Logger logger)
        {
            _importListStatusService = importListStatusService;
            _importListFactory = importListFactory;
            _listFetcherAndParser = listFetcherAndParser;
            _albumSearchService = albumSearchService;
            _artistSearchService = artistSearchService;
            _artistService = artistService;
            _addArtistService = addArtistService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }


        private List<Album> Sync()
        {
            _logger.ProgressInfo("Starting Import List Sync");

            var rssReleases = _listFetcherAndParser.Fetch();

            var reports = rssReleases.ToList();
            var processed = new List<Album>();
            var artistsToAdd = new List<Artist>();

            _logger.ProgressInfo("Processing {0} list items", reports.Count);

            var reportNumber = 1;

            foreach (var report in reports)
            {
                _logger.ProgressTrace("Processing list item {0}/{1}", reportNumber, reports.Count);

                reportNumber++;

                var importList = _importListFactory.Get(report.ImportListId);

                // Map MBid if we only have an album title
                if (report.AlbumMusicBrainzId.IsNullOrWhiteSpace() && report.Album.IsNotNullOrWhiteSpace())
                {
                    var mappedAlbum = _albumSearchService.SearchForNewAlbum(report.Album, report.Artist)
                        .FirstOrDefault();

                    if (mappedAlbum == null) continue; // Break if we are looking for an album and cant find it. This will avoid us from adding the artist and possibly getting it wrong.

                    report.AlbumMusicBrainzId = mappedAlbum.ForeignAlbumId;
                    report.Album = mappedAlbum.Title;
                    report.Artist = mappedAlbum.Artist?.Name;
                    report.ArtistMusicBrainzId = mappedAlbum?.Artist?.ForeignArtistId;

                }

                // Map MBid if we only have a artist name
                if (report.ArtistMusicBrainzId.IsNullOrWhiteSpace() && report.Artist.IsNotNullOrWhiteSpace())
                {
                    var mappedArtist = _artistSearchService.SearchForNewArtist(report.Artist)
                        .FirstOrDefault();
                    report.ArtistMusicBrainzId = mappedArtist?.ForeignArtistId;
                    report.Artist = mappedArtist?.Name;
                }

                // Check to see if artist in DB
                var existingArtist = _artistService.FindById(report.ArtistMusicBrainzId);

                // Append Artist if not already in DB or already on add list
                if (existingArtist == null && artistsToAdd.All(s => s.ForeignArtistId != report.ArtistMusicBrainzId))
                {
                    artistsToAdd.Add(new Artist
                    {
                        ForeignArtistId = report.ArtistMusicBrainzId,
                        Name = report.Artist,
                        Monitored = importList.ShouldMonitor,
                        RootFolderPath = importList.RootFolderPath,
                        ProfileId = importList.ProfileId,
                        LanguageProfileId = importList.LanguageProfileId,
                        MetadataProfileId = importList.MetadataProfileId,
                        AlbumFolder = true,
                        AddOptions = new AddArtistOptions{SearchForMissingAlbums = true, Monitored = importList.ShouldMonitor, SelectedOption = 0}
                    });
                }

                // Add Album so we know what to monitor
                if (report.AlbumMusicBrainzId.IsNotNullOrWhiteSpace() && artistsToAdd.Any(s=>s.ForeignArtistId == report.ArtistMusicBrainzId) && importList.ShouldMonitor)
                {
                    artistsToAdd.Find(s => s.ForeignArtistId == report.ArtistMusicBrainzId).AddOptions.AlbumsToMonitor.Add(report.AlbumMusicBrainzId);
                }
            }

            _addArtistService.AddArtists(artistsToAdd);
            
            var message = string.Format("Import List Sync Completed. Reports found: {0}, Reports grabbed: {1}", reports.Count, processed.Count);

            _logger.ProgressInfo(message);

            return processed;
        }

        public void Execute(ImportListSyncCommand message)
        {
            var processed = Sync();

            _eventAggregator.PublishEvent(new ImportListSyncCompleteEvent(processed));
        }
    }
}
