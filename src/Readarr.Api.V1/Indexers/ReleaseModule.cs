using System;
using System.Collections.Generic;
using FluentValidation;
using Nancy;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Readarr.Api.V1.Indexers
{
    public class ReleaseModule : ReleaseModuleBase
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly Logger _logger;

        private readonly ICached<RemoteBook> _remoteAlbumCache;

        public ReleaseModule(IFetchAndParseRss rssFetcherAndParser,
                             ISearchForNzb nzbSearchService,
                             IMakeDownloadDecision downloadDecisionMaker,
                             IPrioritizeDownloadDecision prioritizeDownloadDecision,
                             IDownloadService downloadService,
                             ICacheManager cacheManager,
                             Logger logger)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _nzbSearchService = nzbSearchService;
            _downloadDecisionMaker = downloadDecisionMaker;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _downloadService = downloadService;
            _logger = logger;

            GetResourceAll = GetReleases;
            Post("/", x => DownloadRelease(ReadResourceFromRequest()));

            PostValidator.RuleFor(s => s.IndexerId).ValidId();
            PostValidator.RuleFor(s => s.Guid).NotEmpty();

            _remoteAlbumCache = cacheManager.GetCache<RemoteBook>(GetType(), "remoteAlbums");
        }

        private object DownloadRelease(ReleaseResource release)
        {
            var remoteAlbum = _remoteAlbumCache.Find(GetCacheKey(release));

            if (remoteAlbum == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Couldn't find requested release in cache, try searching again");
            }

            try
            {
                _downloadService.DownloadReport(remoteAlbum);
            }
            catch (ReleaseDownloadException ex)
            {
                _logger.Error(ex, "Getting release from indexer failed");
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Getting release from indexer failed");
            }

            return release;
        }

        private List<ReleaseResource> GetReleases()
        {
            if (Request.Query.bookId.HasValue)
            {
                return GetAlbumReleases(Request.Query.bookId);
            }

            if (Request.Query.authorId.HasValue)
            {
                return GetArtistReleases(Request.Query.authorId);
            }

            return GetRss();
        }

        private List<ReleaseResource> GetAlbumReleases(int bookId)
        {
            try
            {
                var decisions = _nzbSearchService.AlbumSearch(bookId, true, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Album search failed");
            }

            return new List<ReleaseResource>();
        }

        private List<ReleaseResource> GetArtistReleases(int authorId)
        {
            try
            {
                var decisions = _nzbSearchService.ArtistSearch(authorId, false, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Artist search failed");
            }

            return new List<ReleaseResource>();
        }

        private List<ReleaseResource> GetRss()
        {
            var reports = _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

            return MapDecisions(prioritizedDecisions);
        }

        protected override ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
        {
            var resource = base.MapDecision(decision, initialWeight);
            _remoteAlbumCache.Set(GetCacheKey(resource), decision.RemoteBook, TimeSpan.FromMinutes(30));
            return resource;
        }

        private string GetCacheKey(ReleaseResource resource)
        {
            return string.Concat(resource.IndexerId, "_", resource.Guid);
        }
    }
}
