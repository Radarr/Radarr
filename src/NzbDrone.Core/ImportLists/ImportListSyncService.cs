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
        private readonly IImportListFactory _importListFactory;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly IFetchAndParseImportList _listFetcherAndParser;
        private readonly ISearchForNewAlbum _albumSearchService;
        private readonly ISearchForNewArtist _artistSearchService;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IAddArtistService _addArtistService;
        private readonly IAddAlbumService _addAlbumService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportListSyncService(IImportListFactory importListFactory,
                                     IImportListExclusionService importListExclusionService,
                                     IFetchAndParseImportList listFetcherAndParser,
                                     ISearchForNewAlbum albumSearchService,
                                     ISearchForNewArtist artistSearchService,
                                     IArtistService artistService,
                                     IAlbumService albumService,
                                     IAddArtistService addArtistService,
                                     IAddAlbumService addAlbumService,
                                     IEventAggregator eventAggregator,
                                     Logger logger)
        {
            _importListFactory = importListFactory;
            _importListExclusionService = importListExclusionService;
            _listFetcherAndParser = listFetcherAndParser;
            _albumSearchService = albumSearchService;
            _artistSearchService = artistSearchService;
            _artistService = artistService;
            _albumService = albumService;
            _addArtistService = addArtistService;
            _addAlbumService = addAlbumService;
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
            var albumsToAdd = new List<Album>();

            _logger.ProgressInfo("Processing {0} list items", reports.Count);

            var reportNumber = 1;

            var listExclusions = _importListExclusionService.All();

            foreach (var report in reports)
            {
                _logger.ProgressTrace("Processing list item {0}/{1}", reportNumber, reports.Count);

                reportNumber++;

                var importList = _importListFactory.Get(report.ImportListId);

                if (report.Album.IsNotNullOrWhiteSpace() || report.AlbumMusicBrainzId.IsNotNullOrWhiteSpace())
                {
                    if (report.AlbumMusicBrainzId.IsNullOrWhiteSpace() || report.ArtistMusicBrainzId.IsNullOrWhiteSpace())
                    {
                        MapAlbumReport(report);
                    }

                    ProcessAlbumReport(importList, report, listExclusions, albumsToAdd);
                }
                else if (report.Artist.IsNotNullOrWhiteSpace() || report.ArtistMusicBrainzId.IsNotNullOrWhiteSpace())
                {
                    if (report.ArtistMusicBrainzId.IsNullOrWhiteSpace())
                    {
                        MapArtistReport(report);
                    }

                    ProcessArtistReport(importList, report, listExclusions, artistsToAdd);
                }
            }

            _addArtistService.AddArtists(artistsToAdd);
            _addAlbumService.AddAlbums(albumsToAdd);

            var message = string.Format($"Import List Sync Completed. Items found: {reports.Count}, Artists added: {artistsToAdd.Count}, Albums added: {albumsToAdd.Count}");

            _logger.ProgressInfo(message);

            return processed;
        }

        private void MapAlbumReport(ImportListItemInfo report)
        {
            var albumQuery = report.AlbumMusicBrainzId.IsNotNullOrWhiteSpace() ? $"lidarr:{report.AlbumMusicBrainzId}" : report.Album;
            var mappedAlbum = _albumSearchService.SearchForNewAlbum(albumQuery, report.Artist)
                .FirstOrDefault();

            // Break if we are looking for an album and cant find it. This will avoid us from adding the artist and possibly getting it wrong.
            if (mappedAlbum == null)
            {
                return;
            }

            report.AlbumMusicBrainzId = mappedAlbum.ForeignAlbumId;
            report.Album = mappedAlbum.Title;
            report.Artist = mappedAlbum.ArtistMetadata?.Value?.Name;
            report.ArtistMusicBrainzId = mappedAlbum.ArtistMetadata?.Value?.ForeignArtistId;
        }

        private void ProcessAlbumReport(ImportListDefinition importList, ImportListItemInfo report, List<ImportListExclusion> listExclusions, List<Album> albumsToAdd)
        {
            if (report.AlbumMusicBrainzId == null)
            {
                return;
            }

            // Check to see if album in DB
            var existingAlbum = _albumService.FindById(report.AlbumMusicBrainzId);

            if (existingAlbum != null)
            {
                _logger.Debug("{0} [{1}] Rejected, Album Exists in DB", report.AlbumMusicBrainzId, report.Album);
                return;
            }

            // Check to see if album excluded
            var excludedAlbum = listExclusions.SingleOrDefault(s => s.ForeignId == report.AlbumMusicBrainzId);

            if (excludedAlbum != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.AlbumMusicBrainzId, report.Album);
                return;
            }

            // Check to see if artist excluded
            var excludedArtist = listExclusions.SingleOrDefault(s => s.ForeignId == report.ArtistMusicBrainzId);

            if (excludedArtist != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion for parent artist", report.AlbumMusicBrainzId, report.Album);
                return;
            }

            // Append Album if not already in DB or already on add list
            if (albumsToAdd.All(s => s.ForeignAlbumId != report.AlbumMusicBrainzId))
            {
                var monitored = importList.ShouldMonitor != ImportListMonitorType.None;

                var toAdd = new Album
                {
                    ForeignAlbumId = report.AlbumMusicBrainzId,
                    Monitored = monitored,
                    AnyReleaseOk = true,
                    Artist = new Artist
                    {
                        Monitored = monitored,
                        RootFolderPath = importList.RootFolderPath,
                        QualityProfileId = importList.ProfileId,
                        MetadataProfileId = importList.MetadataProfileId,
                        Tags = importList.Tags,
                        AlbumFolder = true,
                        AddOptions = new AddArtistOptions
                        {
                            SearchForMissingAlbums = monitored,
                            Monitored = monitored,
                            Monitor = monitored ? MonitorTypes.All : MonitorTypes.None
                        }
                    },
                };

                if (importList.ShouldMonitor == ImportListMonitorType.SpecificAlbum)
                {
                    toAdd.Artist.Value.AddOptions.AlbumsToMonitor.Add(toAdd.ForeignAlbumId);
                }

                albumsToAdd.Add(toAdd);
            }
        }

        private void MapArtistReport(ImportListItemInfo report)
        {
            var mappedArtist = _artistSearchService.SearchForNewArtist(report.Artist)
                .FirstOrDefault();
            report.ArtistMusicBrainzId = mappedArtist?.Metadata.Value?.ForeignArtistId;
            report.Artist = mappedArtist?.Metadata.Value?.Name;
        }

        private void ProcessArtistReport(ImportListDefinition importList, ImportListItemInfo report, List<ImportListExclusion> listExclusions, List<Artist> artistsToAdd)
        {
            if (report.ArtistMusicBrainzId == null)
            {
                return;
            }

            // Check to see if artist in DB
            var existingArtist = _artistService.FindById(report.ArtistMusicBrainzId);

            if (existingArtist != null)
            {
                _logger.Debug("{0} [{1}] Rejected, Artist Exists in DB", report.ArtistMusicBrainzId, report.Artist);
                return;
            }

            // Check to see if artist excluded
            var excludedArtist = listExclusions.Where(s => s.ForeignId == report.ArtistMusicBrainzId).SingleOrDefault();

            if (excludedArtist != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.ArtistMusicBrainzId, report.Artist);
                return;
            }

            // Append Artist if not already in DB or already on add list
            if (artistsToAdd.All(s => s.Metadata.Value.ForeignArtistId != report.ArtistMusicBrainzId))
            {
                var monitored = importList.ShouldMonitor != ImportListMonitorType.None;

                artistsToAdd.Add(new Artist
                {
                    Metadata = new ArtistMetadata
                    {
                        ForeignArtistId = report.ArtistMusicBrainzId,
                        Name = report.Artist
                    },
                    Monitored = monitored,
                    RootFolderPath = importList.RootFolderPath,
                    QualityProfileId = importList.ProfileId,
                    MetadataProfileId = importList.MetadataProfileId,
                    Tags = importList.Tags,
                    AlbumFolder = true,
                    AddOptions = new AddArtistOptions
                    {
                        SearchForMissingAlbums = monitored,
                        Monitored = monitored,
                        Monitor = monitored ? MonitorTypes.All : MonitorTypes.None
                    }
                });
            }
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
