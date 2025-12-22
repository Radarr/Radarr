using System;
using System.Collections.Generic;
using NzbDrone.Core.Audiobooks.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaItems;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Audiobooks
{
    public interface IAudiobookService : IBaseMediaService<Audiobook>
    {
        Audiobook GetAudiobook(int audiobookId);
        List<Audiobook> GetAudiobooks(IEnumerable<int> audiobookIds);
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

    public class AudiobookService : BaseMediaService<Audiobook>, IAudiobookService
    {
        private readonly IAudiobookRepository _audiobookRepository;
        private readonly IEventAggregator _eventAggregator;

        public AudiobookService(IAudiobookRepository audiobookRepository, IEventAggregator eventAggregator)
        {
            _audiobookRepository = audiobookRepository;
            _eventAggregator = eventAggregator;
        }

        protected override IBasicRepository<Audiobook> Repository => _audiobookRepository;
        protected override IEventAggregator EventAggregator => _eventAggregator;

        public Audiobook GetAudiobook(int audiobookId) => Get(audiobookId);
        public List<Audiobook> GetAudiobooks(IEnumerable<int> audiobookIds) => Get(audiobookIds);
        public Audiobook AddAudiobook(Audiobook newAudiobook) => Add(newAudiobook);
        public List<Audiobook> AddAudiobooks(List<Audiobook> newAudiobooks) => AddMany(newAudiobooks);
        public void DeleteAudiobook(int audiobookId, bool deleteFiles) => Delete(audiobookId, deleteFiles);
        public void DeleteAudiobooks(List<int> audiobookIds, bool deleteFiles) => DeleteMany(audiobookIds, deleteFiles);
        public List<Audiobook> GetAllAudiobooks() => GetAll();
        public Audiobook UpdateAudiobook(Audiobook audiobook) => Update(audiobook);
        public List<Audiobook> UpdateAudiobooks(List<Audiobook> audiobooks) => UpdateMany(audiobooks);

        public Audiobook FindByIsbn(string isbn) => _audiobookRepository.FindByIsbn(isbn);
        public Audiobook FindByIsbn13(string isbn13) => _audiobookRepository.FindByIsbn13(isbn13);
        public Audiobook FindByAsin(string asin) => _audiobookRepository.FindByAsin(asin);
        public Audiobook FindByForeignId(string foreignAudiobookId) => _audiobookRepository.FindByForeignId(foreignAudiobookId);
        public Audiobook FindByPath(string path) => _audiobookRepository.FindByPath(path);
        public List<Audiobook> FindByAuthorId(int authorId) => _audiobookRepository.FindByAuthorId(authorId);
        public List<Audiobook> FindBySeriesId(int seriesId) => _audiobookRepository.FindBySeriesId(seriesId);
        public List<Audiobook> FindByBookId(int bookId) => _audiobookRepository.FindByBookId(bookId);
        public List<Audiobook> FindByNarrator(string narrator) => _audiobookRepository.FindByNarrator(narrator);
        public Dictionary<int, string> AllAudiobookPaths() => _audiobookRepository.AllAudiobookPaths();
        public List<Audiobook> GetAudiobooksBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
            => _audiobookRepository.AudiobooksBetweenDates(start, end, includeUnmonitored);
        public Dictionary<int, List<int>> AllAudiobookTags() => _audiobookRepository.AllAudiobookTags();
        public bool AudiobookPathExists(string folder) => _audiobookRepository.AudiobookPathExists(folder);

        protected override void OnItemAdded(Audiobook item)
            => _eventAggregator.PublishEvent(new AudiobookAddedEvent(item));

        protected override void OnItemsImported(List<Audiobook> items)
            => _eventAggregator.PublishEvent(new AudiobooksImportedEvent(items));

        protected override void OnItemDeleted(Audiobook item, bool deleteFiles)
            => _eventAggregator.PublishEvent(new AudiobookDeletedEvent(item, deleteFiles));

        protected override void OnItemsDeleted(List<Audiobook> items, bool deleteFiles)
            => _eventAggregator.PublishEvent(new AudiobooksDeletedEvent(items, deleteFiles));

        protected override void OnItemEdited(Audiobook updated, Audiobook stored)
            => _eventAggregator.PublishEvent(new AudiobookEditedEvent(updated, stored));

        protected override void OnItemsBulkEdited(List<Audiobook> items)
            => _eventAggregator.PublishEvent(new AudiobooksBulkEditedEvent(items));
    }
}
