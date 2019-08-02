using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListSyncService : IExecute<ImportListSyncCommand>
    {
        private readonly IImportListStatusService _importListStatusService;
        private readonly IImportListFactory _importListFactory;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly IFetchAndParseImportList _listFetcherAndParser;
        private readonly ISearchForNewAlbum _albumSearchService;
        private readonly ISearchForNewArtist _artistSearchService;
        private readonly IArtistService _artistService;
        private readonly IAddArtistService _addArtistService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportListSyncService(IImportListStatusService importListStatusService,
                              IImportListFactory importListFactory,
                              IImportListExclusionService importListExclusionService,
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
            _importListExclusionService = importListExclusionService;
            _listFetcherAndParser = listFetcherAndParser;
            _albumSearchService = albumSearchService;
            _artistSearchService = artistSearchService;
            _artistService = artistService;
            _addArtistService = addArtistService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }


        private List<Album> SyncAll()
        {
            _logger.ProgressInfo("Starting Import List Sync");

            var rssReleases = _listFetcherAndParser.Fetch();

            var reports = rssReleases.ToList();

            return ProcessReports(reports);

        }

        private List<Album> SyncList(ImportListDefinition definition)
        {
            _logger.ProgressInfo(string.Format("Starting Import List Refresh for List {0}", definition.Name));

            var rssReleases = _listFetcherAndParser.FetchSingleList(definition);

            var reports = rssReleases.ToList();

            return ProcessReports(reports);

        }

        private List<Album> ProcessReports(List<ImportListItemInfo> reports)
        {
            var processed = new List<Album>();
            var artistsToAdd = new List<Artist>();

            _logger.ProgressInfo("Processing {0} list items", reports.Count);

            var reportNumber = 1;

            var listExclusions = _importListExclusionService.All();

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
                    report.Artist = mappedAlbum.ArtistMetadata?.Value?.Name;
                    report.ArtistMusicBrainzId = mappedAlbum?.ArtistMetadata?.Value?.ForeignArtistId;

                }

                // Map MBid if we only have a artist name
                if (report.ArtistMusicBrainzId.IsNullOrWhiteSpace() && report.Artist.IsNotNullOrWhiteSpace())
                {
                    var mappedArtist = _artistSearchService.SearchForNewArtist(report.Artist)
                        .FirstOrDefault();
                    report.ArtistMusicBrainzId = mappedArtist?.Metadata.Value?.ForeignArtistId;
                    report.Artist = mappedArtist?.Metadata.Value?.Name;
                }

                // Check to see if artist in DB
                var existingArtist = _artistService.FindById(report.ArtistMusicBrainzId);
                
                // Check to see if artist excluded
                var excludedArtist = listExclusions.Where(s => s.ForeignId == report.ArtistMusicBrainzId).SingleOrDefault();

                if (excludedArtist != null)
                {
                    _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.ArtistMusicBrainzId, report.Artist);
                }

                // Append Artist if not already in DB or already on add list
                if (existingArtist == null && excludedArtist == null && artistsToAdd.All(s => s.Metadata.Value.ForeignArtistId != report.ArtistMusicBrainzId))
                {
                    var monitored = importList.ShouldMonitor != ImportListMonitorType.None;
                    artistsToAdd.Add(new Artist
                    {
                        Metadata = new ArtistMetadata {
                            ForeignArtistId = report.ArtistMusicBrainzId,
                            Name = report.Artist
                        },
                        Monitored = monitored,
                        RootFolderPath = importList.RootFolderPath,
                        QualityProfileId = importList.ProfileId,
                        MetadataProfileId = importList.MetadataProfileId,
                        Tags = importList.Tags,
                        AlbumFolder = true,
                        AddOptions = new AddArtistOptions {
                            SearchForMissingAlbums = monitored,
                            Monitored = monitored,
                            Monitor = monitored ? MonitorTypes.All : MonitorTypes.None
                        }
                    });
                }

                // Add Album so we know what to monitor
                if (report.AlbumMusicBrainzId.IsNotNullOrWhiteSpace() && artistsToAdd.Any(s => s.Metadata.Value.ForeignArtistId == report.ArtistMusicBrainzId) && importList.ShouldMonitor == ImportListMonitorType.SpecificAlbum)
                {
                    artistsToAdd.Find(s => s.Metadata.Value.ForeignArtistId == report.ArtistMusicBrainzId).AddOptions.AlbumsToMonitor.Add(report.AlbumMusicBrainzId);
                }
            }

            _addArtistService.AddArtists(artistsToAdd);

            var message = string.Format("Import List Sync Completed. Reports found: {0}, Reports grabbed: {1}", reports.Count, processed.Count);

            _logger.ProgressInfo(message);

            return processed;
        }

        public void Execute(ImportListSyncCommand message)
        {
            List<Album> processed;

            if (message.DefinitionId.HasValue)
            {
                processed = SyncList(_importListFactory.Get(message.DefinitionId.Value));
            }
            else
            {
                processed = SyncAll();
            }

            _eventAggregator.PublishEvent(new ImportListSyncCompleteEvent(processed));
        }
    }
}
