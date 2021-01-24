using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Jobs;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Download.Pending
{
    public interface IPendingReleaseService
    {
        void Add(DownloadDecision decision, PendingReleaseReason reason);
        void AddMany(List<Tuple<DownloadDecision, PendingReleaseReason>> decisions);
        List<ReleaseInfo> GetPending();
        List<RemoteBook> GetPendingRemoteBooks(int authorId);
        List<Queue.Queue> GetPendingQueue();
        Queue.Queue FindPendingQueueItem(int queueId);
        void RemovePendingQueueItems(int queueId);
        RemoteBook OldestPendingRelease(int authorId, int[] bookIds);
    }

    public class PendingReleaseService : IPendingReleaseService,
                                         IHandle<AuthorDeletedEvent>,
                                         IHandle<BookGrabbedEvent>,
                                         IHandle<RssSyncCompleteEvent>
    {
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IPendingReleaseRepository _repository;
        private readonly IAuthorService _authorService;
        private readonly IParsingService _parsingService;
        private readonly IDelayProfileService _delayProfileService;
        private readonly ITaskManager _taskManager;
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public PendingReleaseService(IIndexerStatusService indexerStatusService,
                                    IPendingReleaseRepository repository,
                                    IAuthorService authorService,
                                    IParsingService parsingService,
                                    IDelayProfileService delayProfileService,
                                    ITaskManager taskManager,
                                    IConfigService configService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _indexerStatusService = indexerStatusService;
            _repository = repository;
            _authorService = authorService;
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
            foreach (var authorDecisions in decisions.GroupBy(v => v.Item1.RemoteBook.Author.Id))
            {
                var author = authorDecisions.First().Item1.RemoteBook.Author;
                var alreadyPending = _repository.AllByAuthorId(author.Id);

                alreadyPending = IncludeRemoteBooks(alreadyPending, authorDecisions.ToDictionaryIgnoreDuplicates(v => v.Item1.RemoteBook.Release.Title, v => v.Item1.RemoteBook));
                var alreadyPendingByBook = CreateBookLookup(alreadyPending);

                foreach (var pair in authorDecisions)
                {
                    var decision = pair.Item1;
                    var reason = pair.Item2;

                    var bookIds = decision.RemoteBook.Books.Select(e => e.Id);

                    var existingReports = bookIds.SelectMany(v => alreadyPendingByBook[v] ?? Enumerable.Empty<PendingRelease>())
                                                    .Distinct().ToList();

                    var matchingReports = existingReports.Where(MatchingReleasePredicate(decision.RemoteBook.Release)).ToList();

                    if (matchingReports.Any())
                    {
                        var matchingReport = matchingReports.First();

                        if (matchingReport.Reason != reason)
                        {
                            _logger.Debug("The release {0} is already pending with reason {1}, changing to {2}", decision.RemoteBook, matchingReport.Reason, reason);
                            matchingReport.Reason = reason;
                            _repository.Update(matchingReport);
                        }
                        else
                        {
                            _logger.Debug("The release {0} is already pending with reason {1}, not adding again", decision.RemoteBook, reason);
                        }

                        if (matchingReports.Count() > 1)
                        {
                            _logger.Debug("The release {0} had {1} duplicate pending, removing duplicates.", decision.RemoteBook, matchingReports.Count() - 1);

                            foreach (var duplicate in matchingReports.Skip(1))
                            {
                                _repository.Delete(duplicate.Id);
                                alreadyPending.Remove(duplicate);
                                alreadyPendingByBook = CreateBookLookup(alreadyPending);
                            }
                        }

                        continue;
                    }

                    _logger.Debug("Adding release {0} to pending releases with reason {1}", decision.RemoteBook, reason);
                    Insert(decision, reason);
                }
            }
        }

        private ILookup<int, PendingRelease> CreateBookLookup(IEnumerable<PendingRelease> alreadyPending)
        {
            return alreadyPending.SelectMany(v => v.RemoteBook.Books
                    .Select(d => new { Book = d, PendingRelease = v }))
                .ToLookup(v => v.Book.Id, v => v.PendingRelease);
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

        public List<RemoteBook> GetPendingRemoteBooks(int authorId)
        {
            return IncludeRemoteBooks(_repository.AllByAuthorId(authorId)).Select(v => v.RemoteBook).ToList();
        }

        public List<Queue.Queue> GetPendingQueue()
        {
            var queued = new List<Queue.Queue>();

            var nextRssSync = new Lazy<DateTime>(() => _taskManager.GetNextExecution(typeof(RssSyncCommand)));

            var pendingReleases = IncludeRemoteBooks(_repository.WithoutFallback());
            foreach (var pendingRelease in pendingReleases)
            {
                foreach (var book in pendingRelease.RemoteBook.Books)
                {
                    var ect = pendingRelease.Release.PublishDate.AddMinutes(GetDelay(pendingRelease.RemoteBook));

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
                        Id = GetQueueId(pendingRelease, book),
                        Author = pendingRelease.RemoteBook.Author,
                        Book = book,
                        Quality = pendingRelease.RemoteBook.ParsedBookInfo.Quality,
                        Title = pendingRelease.Title,
                        Size = pendingRelease.RemoteBook.Release.Size,
                        Sizeleft = pendingRelease.RemoteBook.Release.Size,
                        RemoteBook = pendingRelease.RemoteBook,
                        Timeleft = timeleft,
                        EstimatedCompletionTime = ect,
                        Status = pendingRelease.Reason.ToString(),
                        Protocol = pendingRelease.RemoteBook.Release.DownloadProtocol,
                        Indexer = pendingRelease.RemoteBook.Release.Indexer
                    };

                    queued.Add(queue);
                }
            }

            //Return best quality release for each book
            var deduped = queued.GroupBy(q => q.Book.Id).Select(g =>
            {
                var author = g.First().Author;

                return g.OrderByDescending(e => e.Quality, new QualityModelComparer(author.QualityProfile))
                        .ThenBy(q => PrioritizeDownloadProtocol(q.Author, q.Protocol))
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
            var authorReleases = _repository.AllByAuthorId(targetItem.AuthorId);

            var releasesToRemove = authorReleases.Where(
                c => c.ParsedBookInfo.BookTitle == targetItem.ParsedBookInfo.BookTitle);

            _repository.DeleteMany(releasesToRemove.Select(c => c.Id));
        }

        public RemoteBook OldestPendingRelease(int authorId, int[] bookIds)
        {
            var authorReleases = GetPendingReleases(authorId);

            return authorReleases.Select(r => r.RemoteBook)
                                 .Where(r => r.Books.Select(e => e.Id).Intersect(bookIds).Any())
                                 .OrderByDescending(p => p.Release.AgeHours)
                                 .FirstOrDefault();
        }

        private List<PendingRelease> GetPendingReleases()
        {
            return IncludeRemoteBooks(_repository.All().ToList());
        }

        private List<PendingRelease> GetPendingReleases(int authorId)
        {
            return IncludeRemoteBooks(_repository.AllByAuthorId(authorId).ToList());
        }

        private List<PendingRelease> IncludeRemoteBooks(List<PendingRelease> releases, Dictionary<string, RemoteBook> knownRemoteBooks = null)
        {
            var result = new List<PendingRelease>();

            var authorMap = new Dictionary<int, Author>();

            if (knownRemoteBooks != null)
            {
                foreach (var author in knownRemoteBooks.Values.Select(v => v.Author))
                {
                    if (!authorMap.ContainsKey(author.Id))
                    {
                        authorMap[author.Id] = author;
                    }
                }
            }

            foreach (var author in _authorService.GetAuthors(releases.Select(v => v.AuthorId).Distinct().Where(v => !authorMap.ContainsKey(v))))
            {
                authorMap[author.Id] = author;
            }

            foreach (var release in releases)
            {
                var author = authorMap.GetValueOrDefault(release.AuthorId);

                // Just in case the author was removed, but wasn't cleaned up yet (housekeeper will clean it up)
                if (author == null)
                {
                    return null;
                }

                List<Book> books;

                RemoteBook knownRemoteBook;
                if (knownRemoteBooks != null && knownRemoteBooks.TryGetValue(release.Release.Title, out knownRemoteBook))
                {
                    books = knownRemoteBook.Books;
                }
                else
                {
                    books = _parsingService.GetBooks(release.ParsedBookInfo, author);
                }

                release.RemoteBook = new RemoteBook
                {
                    Author = author,
                    Books = books,
                    ParsedBookInfo = release.ParsedBookInfo,
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
                AuthorId = decision.RemoteBook.Author.Id,
                ParsedBookInfo = decision.RemoteBook.ParsedBookInfo,
                Release = decision.RemoteBook.Release,
                Title = decision.RemoteBook.Release.Title,
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

        private int GetDelay(RemoteBook remoteBook)
        {
            var delayProfile = _delayProfileService.AllForTags(remoteBook.Author.Tags).OrderBy(d => d.Order).First();
            var delay = delayProfile.GetProtocolDelay(remoteBook.Release.DownloadProtocol);
            var minimumAge = _configService.MinimumAge;

            return new[] { delay, minimumAge }.Max();
        }

        private void RemoveGrabbed(RemoteBook remoteBook)
        {
            var pendingReleases = GetPendingReleases(remoteBook.Author.Id);
            var bookIds = remoteBook.Books.Select(e => e.Id);

            var existingReports = pendingReleases.Where(r => r.RemoteBook.Books.Select(e => e.Id)
                                                             .Intersect(bookIds)
                                                             .Any())
                                                             .ToList();

            if (existingReports.Empty())
            {
                return;
            }

            var profile = remoteBook.Author.QualityProfile.Value;

            foreach (var existingReport in existingReports)
            {
                var compare = new QualityModelComparer(profile).Compare(remoteBook.ParsedBookInfo.Quality,
                                                                        existingReport.RemoteBook.ParsedBookInfo.Quality);

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
                var matching = pending.Where(MatchingReleasePredicate(rejectedRelease.RemoteBook.Release));

                foreach (var pendingRelease in matching)
                {
                    _logger.Debug("Removing previously pending release, as it has now been rejected.");
                    Delete(pendingRelease);
                }
            }
        }

        private PendingRelease FindPendingRelease(int queueId)
        {
            return GetPendingReleases().First(p => p.RemoteBook.Books.Any(e => queueId == GetQueueId(p, e)));
        }

        private int GetQueueId(PendingRelease pendingRelease, Book book)
        {
            return HashConverter.GetHashInt31(string.Format("pending-{0}-book{1}", pendingRelease.Id, book.Id));
        }

        private int PrioritizeDownloadProtocol(Author author, DownloadProtocol downloadProtocol)
        {
            var delayProfile = _delayProfileService.BestForTags(author.Tags);

            if (downloadProtocol == delayProfile.PreferredProtocol)
            {
                return 0;
            }

            return 1;
        }

        public void Handle(AuthorDeletedEvent message)
        {
            _repository.DeleteByAuthorId(message.Author.Id);
        }

        public void Handle(BookGrabbedEvent message)
        {
            RemoveGrabbed(message.Book);
        }

        public void Handle(RssSyncCompleteEvent message)
        {
            RemoveRejected(message.ProcessedDecisions.Rejected);
        }
    }
}
