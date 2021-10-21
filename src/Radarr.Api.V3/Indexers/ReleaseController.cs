using System;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Validation;
using Radarr.Http;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Radarr.Api.V3.Indexers
{
    [V3ApiController]
    public class ReleaseController : ReleaseControllerBase
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        private readonly ICached<RemoteMovie> _remoteMovieCache;

        public ReleaseController(IFetchAndParseRss rssFetcherAndParser,
                             ISearchForNzb nzbSearchService,
                             IMakeDownloadDecision downloadDecisionMaker,
                             IPrioritizeDownloadDecision prioritizeDownloadDecision,
                             IDownloadService downloadService,
                             IMovieService movieService,
                             ICacheManager cacheManager,
                             IProfileService qualityProfileService,
                             Logger logger)
            : base(qualityProfileService)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _nzbSearchService = nzbSearchService;
            _downloadDecisionMaker = downloadDecisionMaker;
            _prioritizeDownloadDecision = prioritizeDownloadDecision;
            _downloadService = downloadService;
            _movieService = movieService;
            _logger = logger;

            PostValidator.RuleFor(s => s.IndexerId).ValidId();
            PostValidator.RuleFor(s => s.Guid).NotEmpty();

            _remoteMovieCache = cacheManager.GetCache<RemoteMovie>(GetType(), "remoteMovies");
        }

        [HttpPost]
        public object DownloadRelease(ReleaseResource release)
        {
            var remoteMovie = _remoteMovieCache.Find(GetCacheKey(release));

            if (remoteMovie == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Couldn't find requested release in cache, try searching again");
            }

            try
            {
                if (remoteMovie.Movie == null)
                {
                    if (release.MovieId.HasValue)
                    {
                        var movie = _movieService.GetMovie(release.MovieId.Value);

                        remoteMovie.Movie = movie;
                    }
                    else
                    {
                        throw new NzbDroneClientException(HttpStatusCode.NotFound, "Unable to find matching movie");
                    }
                }

                _downloadService.DownloadReport(remoteMovie);
            }
            catch (ReleaseDownloadException ex)
            {
                _logger.Error(ex, ex.Message);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Getting release from indexer failed");
            }

            return release;
        }

        [HttpGet]
        public List<ReleaseResource> GetReleases(int? movieId)
        {
            if (movieId.HasValue)
            {
                return GetMovieReleases(movieId.Value);
            }

            return GetRss();
        }

        private List<ReleaseResource> GetMovieReleases(int movieId)
        {
            try
            {
                var decisions = _nzbSearchService.MovieSearch(movieId, true, true);
                var prioritizedDecisions = _prioritizeDownloadDecision.PrioritizeDecisionsForMovies(decisions);

                return MapDecisions(prioritizedDecisions);
            }
            catch (SearchFailedException ex)
            {
                throw new NzbDroneClientException(HttpStatusCode.BadRequest, ex.Message);
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
