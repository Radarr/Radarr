using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Cache;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.AuthorStats
{
    public interface IAuthorStatisticsService
    {
        List<AuthorStatistics> AuthorStatistics();
        AuthorStatistics AuthorStatistics(int authorId);
    }

    public class AuthorStatisticsService : IAuthorStatisticsService,
        IHandle<AuthorUpdatedEvent>,
        IHandle<AuthorDeletedEvent>,
        IHandle<BookAddedEvent>,
        IHandle<BookDeletedEvent>,
        IHandle<BookImportedEvent>,
        IHandle<BookEditedEvent>,
        IHandle<BookFileDeletedEvent>
    {
        private readonly IAuthorStatisticsRepository _authorStatisticsRepository;
        private readonly ICached<List<BookStatistics>> _cache;

        public AuthorStatisticsService(IAuthorStatisticsRepository authorStatisticsRepository,
                                       ICacheManager cacheManager)
        {
            _authorStatisticsRepository = authorStatisticsRepository;
            _cache = cacheManager.GetCache<List<BookStatistics>>(GetType());
        }

        public List<AuthorStatistics> AuthorStatistics()
        {
            var bookStatistics = _cache.Get("AllAuthors", () => _authorStatisticsRepository.AuthorStatistics());

            return bookStatistics.GroupBy(s => s.AuthorId).Select(s => MapAuthorStatistics(s.ToList())).ToList();
        }

        public AuthorStatistics AuthorStatistics(int authorId)
        {
            var stats = _cache.Get(authorId.ToString(), () => _authorStatisticsRepository.AuthorStatistics(authorId));

            if (stats == null || stats.Count == 0)
            {
                return new AuthorStatistics();
            }

            return MapAuthorStatistics(stats);
        }

        private AuthorStatistics MapAuthorStatistics(List<BookStatistics> bookStatistics)
        {
            var authorStatistics = new AuthorStatistics
            {
                BookStatistics = bookStatistics,
                BookCount = bookStatistics.Sum(s => s.BookCount),
                AuthorId = bookStatistics.First().AuthorId,
                BookFileCount = bookStatistics.Sum(s => s.BookFileCount),
                SizeOnDisk = bookStatistics.Sum(s => s.SizeOnDisk)
            };

            return authorStatistics;
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(AuthorUpdatedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Author.Id.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(AuthorDeletedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Author.Id.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookAddedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Book.AuthorId.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookDeletedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Book.AuthorId.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookImportedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Author.Id.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookEditedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.Book.AuthorId.ToString());
        }

        [EventHandleOrder(EventHandleOrder.First)]
        public void Handle(BookFileDeletedEvent message)
        {
            _cache.Remove("AllAuthors");
            _cache.Remove(message.BookFile.Author.Value.Id.ToString());
        }
    }
}
