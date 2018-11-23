using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseService
    {
        void Add(DownloadDecision decision);

        List<ReleaseInfo> GetPending();
        List<RemoteMovie> GetPendingRemoteMovies(int movieId);
        List<Queue.Queue> GetPendingQueue();
        Queue.Queue FindPendingQueueItem(int queueId);
        void RemovePendingQueueItems(int queueId);
        RemoteMovie OldestPendingRelease(int movieId);
    }

    public class PendingReleaseService : IPendingReleaseService,
                                         IHandle<MovieGrabbedEvent>,
                                         IHandle<MovieDeletedEvent>,
                                         IHandle<RssSyncCompleteEvent>
    {
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IPendingReleaseRepository _repository;
        private readonly IMovieService _movieService;
        private readonly IParsingService _parsingService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly ITaskManager _taskManager;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public PendingReleaseService(IIndexerStatusService indexerStatusService,
                                    IPendingReleaseRepository repository,
                                    IMovieService movieService,
                                    IParsingService parsingService,
                                    IDelayProfileService delayProfileService,
                                    ITaskManager taskManager,
                                    IConfigService configService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _indexerStatusService = indexerStatusService;
            _repository = repository;
            _movieService = movieService;
            _parsingService = parsingService;
            _delayProfileService = delayProfileService;
            _taskManager = taskManager;
            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }


        public void Add(DownloadDecision decision)
        {
            var alreadyPending = GetPendingReleases();

            var movieId = decision.RemoteMovie.Movie.Id;

            var existingReports = alreadyPending.Where(r => r.RemoteMovie.Movie.Id == movieId);

            if (existingReports.Any(MatchingReleasePredicate(decision.RemoteMovie.Release)))
            {
                _logger.Debug("This release is already pending, not adding again");
                return;
            }

            _logger.Debug("Adding release to pending releases");
            Insert(decision);
        }

        public List<ReleaseInfo> GetPending()
        {
            var releases = _repository.All().Select(p => p.Release).ToList();

            if (releases.Any())
            {
                releases = FilterBlockedIndexers(releases);
            }

            return releases;
        }

        private List<ReleaseInfo> FilterBlockedIndexers(List<ReleaseInfo> releases)
        {
            var blockedIndexers = new HashSet<int>(_indexerStatusService.GetBlockedIndexers().Select(v => v.IndexerId));

            return releases.Where(release => !blockedIndexers.Contains(release.IndexerId)).ToList();
        }

        public List<RemoteMovie> GetPendingRemoteMovies(int movieId)
        {
            return _repository.AllByMovieId(movieId).Select(GetRemoteMovie).ToList();
        }

        public List<Queue.Queue> GetPendingQueue()
        {
            var queued = new List<Queue.Queue>();

            var nextRssSync = new Lazy<DateTime>(() => _taskManager.GetNextExecution(typeof(RssSyncCommand)));

            foreach (var pendingRelease in GetPendingReleases())
            {
                var ect = pendingRelease.Release.PublishDate.AddMinutes(GetDelay(pendingRelease.RemoteMovie));

                if (ect < nextRssSync.Value)
                {
                    ect = nextRssSync.Value;
                }
                else
                {
                    ect = ect.AddMinutes(_configService.RssSyncInterval);
                }

                var queue = new Queue.Queue
                {
                    Id = GetQueueId(pendingRelease, pendingRelease.RemoteMovie.Movie),
                    Movie = pendingRelease.RemoteMovie.Movie,
                    Quality = pendingRelease.RemoteMovie.ParsedMovieInfo?.Quality ?? new QualityModel(),
                    Title = pendingRelease.Title,
                    Size = pendingRelease.RemoteMovie.Release.Size,
                    Sizeleft = pendingRelease.RemoteMovie.Release.Size,
                    RemoteMovie = pendingRelease.RemoteMovie,
                    Timeleft = ect.Subtract(DateTime.UtcNow),
                    EstimatedCompletionTime = ect,
                    Status = "Pending",
                    Protocol = pendingRelease.RemoteMovie.Release.DownloadProtocol,
                    Indexer = pendingRelease.RemoteMovie.Release.Indexer
                };

                queued.Add(queue);
            }

            //Return best quality release for each movie
            var deduped = queued.GroupBy(q => q.Movie.Id).Select(g =>
            {
                var movies = g.First().Movie;

                return g.OrderByDescending(e => e.Quality, new QualityModelComparer(movies.Profile))
                        .ThenBy(q => PrioritizeDownloadProtocol(q.Movie, q.Protocol))
                        .First();
            });

            return deduped.ToList();
        }

        public Queue.Queue FindPendingQueueItem(int queueId)
        {
            return GetPendingQueue().SingleOrDefault(p => p.Id == queueId);
        }

        public void RemovePendingQueueItems(int queueId)
        {
            var targetItem = FindPendingRelease(queueId);
            var movieReleases = _repository.AllByMovieId(targetItem.MovieId);

            var releasesToRemove = movieReleases.Where(c => c.ParsedMovieInfo.MovieTitle == targetItem.ParsedMovieInfo.MovieTitle);

            _repository.DeleteMany(releasesToRemove.Select(c => c.Id));
        }

        public RemoteMovie OldestPendingRelease(int movieId)
        {
            return GetPendingRemoteMovies(movieId).OrderByDescending(p => p.Release.AgeHours).FirstOrDefault();
        }

        private List<PendingRelease> GetPendingReleases()
        {
            var result = new List<PendingRelease>();

            foreach (var release in _repository.All())
            {
                var remoteMovie = GetRemoteMovie(release);

                if (remoteMovie == null) continue;

                release.RemoteMovie = remoteMovie;

                result.Add(release);
            }

            return result;
        }

        private RemoteMovie GetRemoteMovie(PendingRelease release)
        {
            var movie = _movieService.GetMovie(release.MovieId);

            //Just in case the movie was removed, but wasn't cleaned up yet (housekeeper will clean it up)
            if (movie == null) return null;

            // var episodes = _parsingService.GetMovie(release.ParsedMovieInfo.MovieTitle);

            return new RemoteMovie
            {
                Movie = movie,
                ParsedMovieInfo = release.ParsedMovieInfo,
                Release = release.Release
            };
        }

        private void Insert(DownloadDecision decision)
        {
		var release = new PendingRelease
            	{
	                MovieId = decision.RemoteMovie.Movie.Id,
	                ParsedMovieInfo = decision.RemoteMovie.ParsedMovieInfo,
	                Release = decision.RemoteMovie.Release,
	                Title = decision.RemoteMovie.Release.Title,
	                Added = DateTime.UtcNow
		};
		if (release.ParsedMovieInfo == null)
		{
			_logger.Warn("Pending release {0} does not have ParsedMovieInfo, will cause issues.", release.Title);
		}

	    _repository.Insert(release);

            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
        }

        private void Delete(PendingRelease pendingRelease)
        {
            _repository.Delete(pendingRelease);
            _eventAggregator.PublishEvent(new PendingReleasesUpdatedEvent());
        }

        private static Func<PendingRelease, bool> MatchingReleasePredicate(ReleaseInfo release)
        {
            return p => p.Title == release.Title &&
                   p.Release.PublishDate == release.PublishDate &&
                   p.Release.Indexer == release.Indexer;
        }

        private int GetDelay(RemoteMovie remoteMovie)
        {
            var delayProfile = _delayProfileService.AllForTags(remoteMovie.Movie.Tags).OrderBy(d => d.Order).First();
            var delay = delayProfile.GetProtocolDelay(remoteMovie.Release.DownloadProtocol);
            var minimumAge = _configService.MinimumAge;

            return new[] { delay, minimumAge }.Max();
        }

        private void RemoveGrabbed(RemoteMovie remoteMovie)
        {
            var pendingReleases = GetPendingReleases();


			var existingReports = pendingReleases.Where(r => r.RemoteMovie.Movie.Id == remoteMovie.Movie.Id)
                                                             .ToList();

            if (existingReports.Empty())
            {
                return;
            }

            var profile = remoteMovie.Movie.Profile.Value;

            foreach (var existingReport in existingReports)
            {
                var compare = new QualityModelComparer(profile).Compare(remoteMovie.ParsedMovieInfo.Quality,
                                                                        existingReport.RemoteMovie.ParsedMovieInfo.Quality);

                //Only remove lower/equal quality pending releases
                //It is safer to retry these releases on the next round than remove it and try to re-add it (if its still in the feed)
                if (compare >= 0)
                {
                    _logger.Debug("Removing previously pending release, as it was grabbed.");
                    Delete(existingReport);
                }
            }
        }

        private void RemoveRejected(List<DownloadDecision> rejected)
        {
            _logger.Debug("Removing failed releases from pending");
            var pending = GetPendingReleases();

            foreach (var rejectedRelease in rejected)
            {
                var matching = pending.Where(MatchingReleasePredicate(rejectedRelease.RemoteMovie.Release));

                foreach (var pendingRelease in matching)
                {
                    _logger.Debug("Removing previously pending release, as it has now been rejected.");
                    Delete(pendingRelease);
                }
            }
        }

        private PendingRelease FindPendingRelease(int queueId)
        {
            return GetPendingReleases().First(p => queueId == GetQueueId(p, p.RemoteMovie.Movie));
        }

        private int GetQueueId(PendingRelease pendingRelease, Movie movie)
        {
            return HashConverter.GetHashInt31(string.Format("pending-{0}-movie{1}", pendingRelease.Id, movie.Id));
        }

        private int PrioritizeDownloadProtocol(Movie movie, DownloadProtocol downloadProtocol)
        {
            var delayProfile = _delayProfileService.BestForTags(movie.Tags);

            if (downloadProtocol == delayProfile.PreferredProtocol)
            {
                return 0;
            }

            return 1;
        }

        public void Handle(MovieDeletedEvent message)
        {
            _repository.DeleteByMovieId(message.Movie.Id);
        }

        public void Handle(MovieGrabbedEvent message)
        {
            RemoveGrabbed(message.Movie);
        }

        public void Handle(RssSyncCompleteEvent message)
        {
            RemoveRejected(message.ProcessedDecisions.Rejected);
        }
    }
}
