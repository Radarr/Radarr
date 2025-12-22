using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Audiobooks.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Audiobooks
{
    public interface IAudiobookService
    {
        Audiobook GetAudiobook(int audiobookId);
        List<Audiobook> GetAudiobooks(IEnumerable<int> audiobookIds);
        PagingSpec<Audiobook> Paged(PagingSpec<Audiobook> pagingSpec);
        Audiobook AddAudiobook(Audiobook newAudiobook);
        List<Audiobook> AddAudiobooks(List<Audiobook> newAudiobooks);
        Audiobook FindByIsbn(string isbn);
        Audiobook FindByIsbn13(string isbn13);
        Audiobook FindByAsin(string asin);
        Audiobook FindByForeignId(string foreignAudiobookId);
        Audiobook FindByPath(string path);
        List<Audiobook> FindByAuthorId(int authorId);
        List<Audiobook> FindBySeriesId(int seriesId);
        List<Audiobook> FindByBookId(int bookId);
        List<Audiobook> FindByNarrator(string narrator);
        Dictionary<int, string> AllAudiobookPaths();
        List<Audiobook> GetAudiobooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        void DeleteAudiobook(int audiobookId, bool deleteFiles);
        void DeleteAudiobooks(List<int> audiobookIds, bool deleteFiles);
        List<Audiobook> GetAllAudiobooks();
        Dictionary<int, List<int>> AllAudiobookTags();
        Audiobook UpdateAudiobook(Audiobook audiobook);
        List<Audiobook> UpdateAudiobooks(List<Audiobook> audiobooks);
        bool AudiobookPathExists(string folder);
    }

    public class AudiobookService : IAudiobookService
    {
        private readonly IAudiobookRepository _audiobookRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public AudiobookService(IAudiobookRepository audiobookRepository,
                                IEventAggregator eventAggregator,
                                Logger logger)
        {
            _audiobookRepository = audiobookRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public Audiobook GetAudiobook(int audiobookId)
        {
            return _audiobookRepository.Get(audiobookId);
        }

        public List<Audiobook> GetAudiobooks(IEnumerable<int> audiobookIds)
        {
            return _audiobookRepository.Get(audiobookIds).ToList();
        }

        public PagingSpec<Audiobook> Paged(PagingSpec<Audiobook> pagingSpec)
        {
            return _audiobookRepository.GetPaged(pagingSpec);
        }

        public Audiobook AddAudiobook(Audiobook newAudiobook)
        {
            newAudiobook.Added = DateTime.UtcNow;
            var audiobook = _audiobookRepository.Insert(newAudiobook);

            _eventAggregator.PublishEvent(new AudiobookAddedEvent(GetAudiobook(audiobook.Id)));

            return audiobook;
        }

        public List<Audiobook> AddAudiobooks(List<Audiobook> newAudiobooks)
        {
            var now = DateTime.UtcNow;
            foreach (var audiobook in newAudiobooks)
            {
                audiobook.Added = now;
            }

            _audiobookRepository.InsertMany(newAudiobooks);

            _eventAggregator.PublishEvent(new AudiobooksImportedEvent(newAudiobooks));

            return newAudiobooks;
        }

        public Audiobook FindByIsbn(string isbn)
        {
            return _audiobookRepository.FindByIsbn(isbn);
        }

        public Audiobook FindByIsbn13(string isbn13)
        {
            return _audiobookRepository.FindByIsbn13(isbn13);
        }

        public Audiobook FindByAsin(string asin)
        {
            return _audiobookRepository.FindByAsin(asin);
        }

        public Audiobook FindByForeignId(string foreignAudiobookId)
        {
            return _audiobookRepository.FindByForeignId(foreignAudiobookId);
        }

        public Audiobook FindByPath(string path)
        {
            return _audiobookRepository.FindByPath(path);
        }

        public List<Audiobook> FindByAuthorId(int authorId)
        {
            return _audiobookRepository.FindByAuthorId(authorId);
        }

        public List<Audiobook> FindBySeriesId(int seriesId)
        {
            return _audiobookRepository.FindBySeriesId(seriesId);
        }

        public List<Audiobook> FindByBookId(int bookId)
        {
            return _audiobookRepository.FindByBookId(bookId);
        }

        public List<Audiobook> FindByNarrator(string narrator)
        {
            return _audiobookRepository.FindByNarrator(narrator);
        }

        public Dictionary<int, string> AllAudiobookPaths()
        {
            return _audiobookRepository.AllAudiobookPaths();
        }

        public List<Audiobook> GetAudiobooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            return _audiobookRepository.AudiobooksBetweenDates(start, end, includeUnmonitored);
        }

        public void DeleteAudiobook(int audiobookId, bool deleteFiles)
        {
            var audiobook = _audiobookRepository.Get(audiobookId);
            _audiobookRepository.Delete(audiobookId);
            _eventAggregator.PublishEvent(new AudiobookDeletedEvent(audiobook, deleteFiles));
        }

        public void DeleteAudiobooks(List<int> audiobookIds, bool deleteFiles)
        {
            var audiobooks = _audiobookRepository.Get(audiobookIds).ToList();
            _audiobookRepository.DeleteMany(audiobookIds);
            _eventAggregator.PublishEvent(new AudiobooksDeletedEvent(audiobooks, deleteFiles));
        }

        public List<Audiobook> GetAllAudiobooks()
        {
            return _audiobookRepository.All().ToList();
        }

        public Dictionary<int, List<int>> AllAudiobookTags()
        {
            return _audiobookRepository.AllAudiobookTags();
        }

        public Audiobook UpdateAudiobook(Audiobook audiobook)
        {
            var storedAudiobook = GetAudiobook(audiobook.Id);
            var updatedAudiobook = _audiobookRepository.Update(audiobook);

            _eventAggregator.PublishEvent(new AudiobookEditedEvent(updatedAudiobook, storedAudiobook));

            return updatedAudiobook;
        }

        public List<Audiobook> UpdateAudiobooks(List<Audiobook> audiobooks)
        {
            _audiobookRepository.UpdateMany(audiobooks);

            _eventAggregator.PublishEvent(new AudiobooksBulkEditedEvent(audiobooks));

            return audiobooks;
        }

        public bool AudiobookPathExists(string folder)
        {
            return _audiobookRepository.AudiobookPathExists(folder);
        }
    }
}
