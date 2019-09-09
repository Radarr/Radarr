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
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseService
    {
        void Add(DownloadDecision decision, PendingReleaseReason reason);
        void AddMany(List<Tuple<DownloadDecision, PendingReleaseReason>> decisions);
        List<ReleaseInfo> GetPending();
        List<RemoteAlbum> GetPendingRemoteAlbums(int artistId);
        List<Queue.Queue> GetPendingQueue();
        Queue.Queue FindPendingQueueItem(int queueId);
        void RemovePendingQueueItems(int queueId);
        RemoteAlbum OldestPendingRelease(int artistId, int[] albumIds);
    }

    public class PendingReleaseService : IPendingReleaseService,
                                         IHandle<ArtistDeletedEvent>,
                                         IHandle<AlbumGrabbedEvent>,
                                         IHandle<RssSyncCompleteEvent>
    {
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IPendingReleaseRepository _repository;
        private readonly IArtistService _artistService;
        private readonly IParsingService _parsingService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly ITaskManager _taskManager;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public PendingReleaseService(IIndexerStatusService indexerStatusService,
                                    IPendingReleaseRepository repository,
                                    IArtistService artistService,
                                    IParsingService parsingService,
                                    IDelayProfileService delayProfileService,
                                    ITaskManager taskManager,
                                    IConfigService configService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _indexerStatusService = indexerStatusService;
            _repository = repository;
            _artistService = artistService;
            _parsingService = parsingService;
            _delayProfileService = delayProfileService;
            _taskManager = taskManager;
            _configService = configService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }


        public void Add(DownloadDecision decision, PendingReleaseReason reason)
        {
            AddMany(new List<Tuple<DownloadDecision, PendingReleaseReason>> { Tuple.Create(decision, reason) });
        }

        public void AddMany(List<Tuple<DownloadDecision, PendingReleaseReason>> decisions)
        {
            foreach (var artistDecisions in decisions.GroupBy(v => v.Item1.RemoteAlbum.Artist.Id))
            {
                var artist = artistDecisions.First().Item1.RemoteAlbum.Artist;
                var alreadyPending = _repository.AllByArtistId(artist.Id);

                alreadyPending = IncludeRemoteAlbums(alreadyPending, artistDecisions.ToDictionaryIgnoreDuplicates(v => v.Item1.RemoteAlbum.Release.Title, v => v.Item1.RemoteAlbum));
                var alreadyPendingByAlbum = CreateAlbumLookup(alreadyPending);

                foreach (var pair in artistDecisions)
                {
                    var decision = pair.Item1;
                    var reason = pair.Item2;

                    var albumIds = decision.RemoteAlbum.Albums.Select(e => e.Id);

                    var existingReports = albumIds.SelectMany(v => alreadyPendingByAlbum[v] ?? Enumerable.Empty<PendingRelease>())
                                                    .Distinct().ToList();

                    var matchingReports = existingReports.Where(MatchingReleasePredicate(decision.RemoteAlbum.Release)).ToList();

                    if (matchingReports.Any())
                    {
                        var matchingReport = matchingReports.First();

                        if (matchingReport.Reason != reason)
                        {
                            _logger.Debug("The release {0} is already pending with reason {1}, changing to {2}", decision.RemoteAlbum, matchingReport.Reason, reason);
                            matchingReport.Reason = reason;
                            _repository.Update(matchingReport);
                        }
                        else
                        {
                            _logger.Debug("The release {0} is already pending with reason {1}, not adding again", decision.RemoteAlbum, reason);
                        }

                        if (matchingReports.Count() > 1)
                        {
                            _logger.Debug("The release {0} had {1} duplicate pending, removing duplicates.", decision.RemoteAlbum, matchingReports.Count() - 1);

                            foreach (var duplicate in matchingReports.Skip(1))
                            {
                                _repository.Delete(duplicate.Id);
                                alreadyPending.Remove(duplicate);
                                alreadyPendingByAlbum = CreateAlbumLookup(alreadyPending);
                            }
                        }

                        continue;
                    }

                    _logger.Debug("Adding release {0} to pending releases with reason {1}", decision.RemoteAlbum, reason);
                    Insert(decision, reason);
                }
            }
        }

        private ILookup<int, PendingRelease> CreateAlbumLookup(IEnumerable<PendingRelease> alreadyPending)
        {
            return alreadyPending.SelectMany(v => v.RemoteAlbum.Albums
                    .Select(d => new { Album = d, PendingRelease = v }))
                .ToLookup(v => v.Album.Id, v => v.PendingRelease);
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
            var blockedIndexers = new HashSet<int>(_indexerStatusService.GetBlockedProviders().Select(v => v.ProviderId));

            return releases.Where(release => !blockedIndexers.Contains(release.IndexerId)).ToList();
        }

        public List<RemoteAlbum> GetPendingRemoteAlbums(int artistId)
        {
            return IncludeRemoteAlbums(_repository.AllByArtistId(artistId)).Select(v => v.RemoteAlbum).ToList();
        }

        public List<Queue.Queue> GetPendingQueue()
        {
            var queued = new List<Queue.Queue>();

            var nextRssSync = new Lazy<DateTime>(() => _taskManager.GetNextExecution(typeof(RssSyncCommand)));

            var pendingReleases = IncludeRemoteAlbums(_repository.WithoutFallback());
            foreach (var pendingRelease in pendingReleases)
            {
                foreach (var album in pendingRelease.RemoteAlbum.Albums)
                {
                    var ect = pendingRelease.Release.PublishDate.AddMinutes(GetDelay(pendingRelease.RemoteAlbum));

                    if (ect < nextRssSync.Value)
                    {
                        ect = nextRssSync.Value;
                    }
                    else
                    {
                        ect = ect.AddMinutes(_configService.RssSyncInterval);
                    }

                    var timeleft = ect.Subtract(DateTime.UtcNow);

                    if (timeleft.TotalSeconds < 0)
                    {
                        timeleft = TimeSpan.Zero;
                    }

                    var queue = new Queue.Queue
                    {
                        Id = GetQueueId(pendingRelease, album),
                        Artist = pendingRelease.RemoteAlbum.Artist,
                        Album = album,
                        Quality = pendingRelease.RemoteAlbum.ParsedAlbumInfo.Quality,
                        Title = pendingRelease.Title,
                        Size = pendingRelease.RemoteAlbum.Release.Size,
                        Sizeleft = pendingRelease.RemoteAlbum.Release.Size,
                        RemoteAlbum = pendingRelease.RemoteAlbum,
                        Timeleft = timeleft,
                        EstimatedCompletionTime = ect,
                        Status = pendingRelease.Reason.ToString(),
                        Protocol = pendingRelease.RemoteAlbum.Release.DownloadProtocol,
                        Indexer = pendingRelease.RemoteAlbum.Release.Indexer
                    };

                    queued.Add(queue);
                }
            }

            //Return best quality release for each album
            var deduped = queued.GroupBy(q => q.Album.Id).Select(g =>
            {
                var artist = g.First().Artist;

                return g.OrderByDescending(e => e.Quality, new QualityModelComparer(artist.QualityProfile))
                        .ThenBy(q => PrioritizeDownloadProtocol(q.Artist, q.Protocol))
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
            var artistReleases = _repository.AllByArtistId(targetItem.ArtistId);

            var releasesToRemove = artistReleases.Where(
                c => c.ParsedAlbumInfo.AlbumTitle == targetItem.ParsedAlbumInfo.AlbumTitle);

            _repository.DeleteMany(releasesToRemove.Select(c => c.Id));
        }

        public RemoteAlbum OldestPendingRelease(int artistId, int[] albumIds)
        {
            var artistReleases = GetPendingReleases(artistId);

            return artistReleases.Select(r => r.RemoteAlbum)
                                 .Where(r => r.Albums.Select(e => e.Id).Intersect(albumIds).Any())
                                 .OrderByDescending(p => p.Release.AgeHours)
                                 .FirstOrDefault();
        }

        private List<PendingRelease> GetPendingReleases()
        {
            return IncludeRemoteAlbums(_repository.All().ToList());
        }

        private List<PendingRelease> GetPendingReleases(int artistId)
        {
            return IncludeRemoteAlbums(_repository.AllByArtistId(artistId).ToList());
        }

        private List<PendingRelease> IncludeRemoteAlbums(List<PendingRelease> releases, Dictionary<string, RemoteAlbum> knownRemoteAlbums = null)
        {
            var result = new List<PendingRelease>();

            var artistMap = new Dictionary<int, Artist>();

            if (knownRemoteAlbums != null)
            {
                foreach (var artist in knownRemoteAlbums.Values.Select(v => v.Artist))
                {
                    if (!artistMap.ContainsKey(artist.Id))
                    {
                        artistMap[artist.Id] = artist;
                    }
                }
            }

            foreach (var artist in _artistService.GetArtists(releases.Select(v => v.ArtistId).Distinct().Where(v => !artistMap.ContainsKey(v))))
            {
                artistMap[artist.Id] = artist;
            }

            foreach (var release in releases)
            {
                var artist = artistMap.GetValueOrDefault(release.ArtistId);

                // Just in case the artist was removed, but wasn't cleaned up yet (housekeeper will clean it up)
                if (artist == null) return null;

                List<Album> albums;

                RemoteAlbum knownRemoteAlbum;
                if (knownRemoteAlbums != null && knownRemoteAlbums.TryGetValue(release.Release.Title, out knownRemoteAlbum))
                {
                    albums = knownRemoteAlbum.Albums;
                }
                else
                {
                    albums = _parsingService.GetAlbums(release.ParsedAlbumInfo, artist);
                }

                release.RemoteAlbum = new RemoteAlbum
                {
                    Artist = artist,
                    Albums = albums,
                    ParsedAlbumInfo = release.ParsedAlbumInfo,
                    Release = release.Release
                };

                result.Add(release);
            }

            return result;
        }

        private void Insert(DownloadDecision decision, PendingReleaseReason reason)
        {
            _repository.Insert(new PendingRelease
            {
                ArtistId = decision.RemoteAlbum.Artist.Id,
                ParsedAlbumInfo = decision.RemoteAlbum.ParsedAlbumInfo,
                Release = decision.RemoteAlbum.Release,
                Title = decision.RemoteAlbum.Release.Title,
                Added = DateTime.UtcNow,
                Reason = reason
            });

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

        private int GetDelay(RemoteAlbum remoteAlbum)
        {
            var delayProfile = _delayProfileService.AllForTags(remoteAlbum.Artist.Tags).OrderBy(d => d.Order).First();
            var delay = delayProfile.GetProtocolDelay(remoteAlbum.Release.DownloadProtocol);
            var minimumAge = _configService.MinimumAge;

            return new[] { delay, minimumAge }.Max();
        }

        private void RemoveGrabbed(RemoteAlbum remoteAlbum)
        {
            var pendingReleases = GetPendingReleases(remoteAlbum.Artist.Id);
            var albumIds = remoteAlbum.Albums.Select(e => e.Id);

            var existingReports = pendingReleases.Where(r => r.RemoteAlbum.Albums.Select(e => e.Id)
                                                             .Intersect(albumIds)
                                                             .Any())
                                                             .ToList();

            if (existingReports.Empty())
            {
                return;
            }

            var profile = remoteAlbum.Artist.QualityProfile.Value;

            foreach (var existingReport in existingReports)
            {
                var compare = new QualityModelComparer(profile).Compare(remoteAlbum.ParsedAlbumInfo.Quality,
                                                                        existingReport.RemoteAlbum.ParsedAlbumInfo.Quality);

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
                var matching = pending.Where(MatchingReleasePredicate(rejectedRelease.RemoteAlbum.Release));

                foreach (var pendingRelease in matching)
                {
                    _logger.Debug("Removing previously pending release, as it has now been rejected.");
                    Delete(pendingRelease);
                }
            }
        }

        private PendingRelease FindPendingRelease(int queueId)
        {
            return GetPendingReleases().First(p => p.RemoteAlbum.Albums.Any(e => queueId == GetQueueId(p, e)));
        }

        private int GetQueueId(PendingRelease pendingRelease, Album album)
        {
            return HashConverter.GetHashInt31(string.Format("pending-{0}-album{1}", pendingRelease.Id, album.Id));
        }

        private int PrioritizeDownloadProtocol(Artist artist, DownloadProtocol downloadProtocol)
        {
            var delayProfile = _delayProfileService.BestForTags(artist.Tags);

            if (downloadProtocol == delayProfile.PreferredProtocol)
            {
                return 0;
            }

            return 1;
        }

        public void Handle(ArtistDeletedEvent message)
        {
            _repository.DeleteByArtistId(message.Artist.Id);
        }

        public void Handle(AlbumGrabbedEvent message)
        {
            RemoveGrabbed(message.Album);
        }

        public void Handle(RssSyncCompleteEvent message)
        {
            RemoveRejected(message.ProcessedDecisions.Rejected);
        }
    }
}
