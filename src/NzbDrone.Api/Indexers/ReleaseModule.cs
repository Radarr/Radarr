using System;
using System.Collections.Generic;
using FluentValidation;
using Nancy;
using NLog;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using Nancy.ModelBinding;
using NzbDrone.Api.Extensions;
using NzbDrone.Common.Cache;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace NzbDrone.Api.Indexers
{
    public class ReleaseModule : ReleaseModuleBase
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly Logger _logger;

        private readonly ICached<RemoteAlbum> _remoteAlbumCache;

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
            Post["/"] = x => DownloadRelease(this.Bind<ReleaseResource>());

            PostValidator.RuleFor(s => s.DownloadAllowed).Equal(true);
            PostValidator.RuleFor(s => s.Guid).NotEmpty();

            _remoteAlbumCache = cacheManager.GetCache<RemoteAlbum>(GetType(), "remoteAlbums");
        }

        private Response DownloadRelease(ReleaseResource release)
        {
            var remoteAlbum = _remoteAlbumCache.Find(release.Guid);

            if (remoteAlbum == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                return new NotFoundResponse();
            }

            try
            {
                _downloadService.DownloadReport(remoteAlbum);
            }
            catch (ReleaseDownloadException ex)
            {
                _logger.Error(ex);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Getting release from indexer failed");
            }

            return release.AsResponse();
        }

        private List<ReleaseResource> GetReleases()
        {
            if (Request.Query.albumId != null)
            {
                return GetAlbumReleases(Request.Query.albumId);
            }

            return GetRss();
        }

        private List<ReleaseResource> GetAlbumReleases(int albumId)
        {
            try
            {
                var decisions = _nzbSearchService.AlbumSearch(albumId, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisions(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Album search failed");
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
            _remoteAlbumCache.Set(decision.RemoteAlbum.Release.Guid, decision.RemoteAlbum, TimeSpan.FromMinutes(30));
           return base.MapDecision(decision, initialWeight);
        }
    }
}