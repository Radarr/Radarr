using System;
using System.Collections.Generic;
using FluentValidation;
using Nancy;
using Nancy.ModelBinding;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Validation;
using Radarr.Http.Extensions;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Radarr.Api.V2.Indexers
{
    public class ReleaseModule : ReleaseModuleBase
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly Logger _logger;

        private readonly ICached<RemoteMovie> _remoteMovieCache;

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
            PostValidator.RuleFor(s => s.IndexerId).ValidId();
            PostValidator.RuleFor(s => s.Guid).NotEmpty();

            _remoteMovieCache = cacheManager.GetCache<RemoteMovie>(GetType(), "remoteMovies");
        }

        private Response DownloadRelease(ReleaseResource release)
        {
            var remoteMovie = _remoteMovieCache.Find(GetCacheKey(release));

            if (remoteMovie == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Couldn't find requested release in cache, try searching again");
            }

            try
            {
                _downloadService.DownloadReport(remoteMovie, false);
            }
            catch (ReleaseDownloadException ex)
            {
                _logger.ErrorException(ex.Message, ex);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Getting release from indexer failed");
            }

            return release.AsResponse();
        }

        private List<ReleaseResource> GetReleases()
        {
            if (Request.Query.movieId.HasValue)
            {
                return GetMovieReleases(Request.Query.movieId);
            }

            return GetRss();
        }

        private List<ReleaseResource> GetMovieReleases(int episodeId)
        {
            try
            {
                var decisions = _nzbSearchService.MovieSearch(episodeId, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisionsForMovies(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Movie search failed: " + ex.Message);
            }

            return new List<ReleaseResource>();
        }

        private List<ReleaseResource> GetRss()
        {
            var reports = _rssFetcherAndParser.Fetch();
            var decisions = _downloadDecisionMaker.GetRssDecision(reports);
            var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisionsForMovies(decisions);

            return MapDecisions(prioritizedDecisions);
        }

        protected override ReleaseResource MapDecision(DownloadDecision decision, int initialWeight)
        {
            var resource = base.MapDecision(decision, initialWeight);
            _remoteMovieCache.Set(GetCacheKey(resource), decision.RemoteMovie, TimeSpan.FromMinutes(30));

            return resource;
        }

        private string GetCacheKey(ReleaseResource resource)
        {
            return string.Concat(resource.IndexerId, "_", resource.Guid);
        }
    }
}
