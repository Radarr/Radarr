using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.IndexerSearch
{
    class AlbumSearchService : IExecute<AlbumSearchCommand>,
                               IExecute<MissingAlbumSearchCommand>,
                               IExecute<CutoffUnmetAlbumSearchCommand>
    {
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IAlbumService _albumService;
        private readonly IAlbumCutoffService _albumCutoffService;
        private readonly IQueueService _queueService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly Logger _logger;

        public AlbumSearchService(ISearchForNzb nzbSearchService,
            IAlbumService albumService,
            IAlbumCutoffService albumCutoffService,
            IQueueService queueService,
            IProcessDownloadDecisions processDownloadDecisions,
            Logger logger)
        {
            _nzbSearchService = nzbSearchService;
            _albumService = albumService;
            _albumCutoffService = albumCutoffService;
            _queueService = queueService;
            _processDownloadDecisions = processDownloadDecisions;
            _logger = logger;
        }

        private void SearchForMissingAlbums(List<Album> albums, bool userInvokedSearch)
        {
            _logger.ProgressInfo("Performing missing search for {0} albums", albums.Count);
            var downloadedCount = 0;

            foreach (var album in albums)
            {
                List<DownloadDecision> decisions;
                decisions = _nzbSearchService.AlbumSearch(album.Id, false, userInvokedSearch, false);
                var processed = _processDownloadDecisions.ProcessDecisions(decisions);

                downloadedCount += processed.Grabbed.Count;
            }

            _logger.ProgressInfo("Completed missing search for {0} albums. {1} reports downloaded.", albums.Count, downloadedCount);
        }


        public void Execute(AlbumSearchCommand message)
        {
            foreach (var albumId in message.AlbumIds)
            {
                var decisions =
                    _nzbSearchService.AlbumSearch(albumId, false, message.Trigger == CommandTrigger.Manual, false);
                var processed = _processDownloadDecisions.ProcessDecisions(decisions);

                _logger.ProgressInfo("Album search completed. {0} reports downloaded.", processed.Grabbed.Count);
            }
        }

        public void Execute(MissingAlbumSearchCommand message)
        {
            List<Album> albums;

            if (message.ArtistId.HasValue)
            {
                int artistId = message.ArtistId.Value;

                albums = _albumService.AlbumsWithoutFiles(new PagingSpec<Album>
                {
                    Page = 1,
                    PageSize = 100000,
                    SortDirection = SortDirection.Ascending,
                    SortKey = "Id",
                    FilterExpression =
                                                            v =>
                                                            v.Monitored == true &&
                                                            v.Artist.Monitored == true
                }).Records.Where(e => e.ArtistId.Equals(artistId)).ToList();
            }

            else
            {
                albums = _albumService.AlbumsWithoutFiles(new PagingSpec<Album>
                {
                    Page = 1,
                    PageSize = 100000,
                    SortDirection = SortDirection.Ascending,
                    SortKey = "Id",
                    FilterExpression =
                                                                            v =>
                                                                            v.Monitored == true &&
                                                                            v.Artist.Monitored == true
                }).Records.ToList();
            }

            var queue = _queueService.GetQueue().Select(q => q.Album.Id);
            var missing = albums.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForMissingAlbums(missing, message.Trigger == CommandTrigger.Manual);
        }

        public void Execute(CutoffUnmetAlbumSearchCommand message)
        {
            Expression<Func<Album, bool>> filterExpression;

            if (message.ArtistId.HasValue)
            {
                filterExpression = v =>
                    v.ArtistId == message.ArtistId.Value &&
                    v.Monitored == true &&
                    v.Artist.Monitored == true;
            }

            else
            {
                filterExpression = v =>
                    v.Monitored == true &&
                    v.Artist.Monitored == true;
            }

            var albums = _albumCutoffService.AlbumsWhereCutoffUnmet(new PagingSpec<Album>
            {
                Page = 1,
                PageSize = 100000,
                SortDirection = SortDirection.Ascending,
                SortKey = "Id",
                FilterExpression = filterExpression
            }).Records.ToList();

            var queue = _queueService.GetQueue().Select(q => q.Album.Id);
            var missing = albums.Where(e => !queue.Contains(e.Id)).ToList();

            SearchForMissingAlbums(missing, message.Trigger == CommandTrigger.Manual);
        }
    }
}
