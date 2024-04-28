using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Validation;
using Radarr.Http;
using HttpStatusCode = System.Net.HttpStatusCode;

namespace Radarr.Api.V3.Indexers
{
    [V3ApiController]
    public class ReleaseController : ReleaseControllerBase
    {
        private readonly IFetchAndParseRss _rssFetcherAndParser;
        private readonly ISearchForReleases _releaseSearchService;
        private readonly IMakeDownloadDecision _downloadDecisionMaker;
        private readonly IPrioritizeDownloadDecision _prioritizeDownloadDecision;
        private readonly IDownloadService _downloadService;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        private readonly ICached<RemoteMovie> _remoteMovieCache;

        public ReleaseController(IFetchAndParseRss rssFetcherAndParser,
                             ISearchForReleases releaseSearchService,
                             IMakeDownloadDecision downloadDecisionMaker,
                             IPrioritizeDownloadDecision prioritizeDownloadDecision,
                             IDownloadService downloadService,
                             IMovieService movieService,
                             ICacheManager cacheManager,
                             IQualityProfileService qualityProfileService,
                             Logger logger)
            : base(qualityProfileService)
        {
            _rssFetcherAndParser = rssFetcherAndParser;
            _releaseSearchService = releaseSearchService;
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
        [Consumes("application/json")]
        public async Task<object> DownloadRelease([FromBody] ReleaseResource release)
        {
            var remoteMovie = _remoteMovieCache.Find(GetCacheKey(release));

            if (remoteMovie == null)
            {
                _logger.Debug("Couldn't find requested release in cache, cache timeout probably expired.");

                throw new NzbDroneClientException(HttpStatusCode.NotFound, "Couldn't find requested release in cache, try searching again");
            }

            try
            {
                if (release.ShouldOverride == true)
                {
                    Ensure.That(release.MovieId, () => release.MovieId).IsNotNull();
                    Ensure.That(release.Quality, () => release.Quality).IsNotNull();
                    Ensure.That(release.Languages, () => release.Languages).IsNotNull();

                    // Clone the remote episode so we don't overwrite anything on the original
                    remoteMovie = new RemoteMovie
                    {
                        Release = remoteMovie.Release,
                        ParsedMovieInfo = remoteMovie.ParsedMovieInfo.JsonClone(),
                        DownloadAllowed = remoteMovie.DownloadAllowed,
                        SeedConfiguration = remoteMovie.SeedConfiguration,
                        CustomFormats = remoteMovie.CustomFormats,
                        CustomFormatScore = remoteMovie.CustomFormatScore,
                        MovieMatchType = remoteMovie.MovieMatchType,
                        ReleaseSource = remoteMovie.ReleaseSource
                    };

                    remoteMovie.Movie = _movieService.GetMovie(release.MovieId!.Value);
                    remoteMovie.ParsedMovieInfo.Quality = release.Quality;
                    remoteMovie.Languages = release.Languages;
                }

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

                await _downloadService.DownloadReport(remoteMovie, release.DownloadClientId);
            }
            catch (ReleaseDownloadException ex)
            {
                _logger.Error(ex, ex.Message);
                throw new NzbDroneClientException(HttpStatusCode.Conflict, "Getting release from indexer failed");
            }

            return release;
        }

        [HttpGet]
        [Produces("application/json")]
        public async Task<List<ReleaseResource>> GetReleases(int? movieId)
        {
            if (movieId.HasValue)
            {
                return await GetMovieReleases(movieId.Value);
            }

            return await GetRss();
        }

        private async Task<List<ReleaseResource>> GetMovieReleases(int movieId)
        {
            try
            {
                var decisions = await _releaseSearchService.MovieSearch(movieId, true, true);
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
                throw new NzbDroneClientException(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private async Task<List<ReleaseResource>> GetRss()
        {
            var reports = await _rssFetcherAndParser.Fetch();
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
