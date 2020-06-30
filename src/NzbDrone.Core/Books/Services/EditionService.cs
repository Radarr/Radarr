using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Books
{
    public interface IEditionService
    {
        Edition GetEdition(int id);
        Edition GetEditionByForeignEditionId(string foreignEditionId);
        List<Edition> GetAllEditions();
        void InsertMany(List<Edition> editions);
        void UpdateMany(List<Edition> editions);
        void DeleteMany(List<Edition> editions);
        List<Edition> GetEditionsForRefresh(int albumId, IEnumerable<string> foreignEditionIds);
        List<Edition> GetEditionsByBook(int bookId);
        List<Edition> GetEditionsByAuthor(int authorId);
        List<Edition> SetMonitored(Edition edition);
    }

    public class EditionService : IEditionService,
        IHandle<BookDeletedEvent>
    {
        private readonly IEditionRepository _editionRepository;
        private readonly IEventAggregator _eventAggregator;

        public EditionService(IEditionRepository editionRepository,
                              IEventAggregator eventAggregator)
        {
            _editionRepository = editionRepository;
            _eventAggregator = eventAggregator;
        }

        public Edition GetEdition(int id)
        {
            return _editionRepository.Get(id);
        }

        public Edition GetEditionByForeignEditionId(string foreignEditionId)
        {
            return _editionRepository.FindByForeignEditionId(foreignEditionId);
        }

        public List<Edition> GetAllEditions()
        {
            return _editionRepository.All().ToList();
        }

        public void InsertMany(List<Edition> editions)
        {
            _editionRepository.InsertMany(editions);
        }

        public void UpdateMany(List<Edition> editions)
        {
            _editionRepository.UpdateMany(editions);
        }

        public void DeleteMany(List<Edition> editions)
        {
            _editionRepository.DeleteMany(editions);
            foreach (var edition in editions)
            {
                _eventAggregator.PublishEvent(new EditionDeletedEvent(edition));
            }
        }

        public List<Edition> GetEditionsForRefresh(int albumId, IEnumerable<string> foreignEditionIds)
        {
            return _editionRepository.GetEditionsForRefresh(albumId, foreignEditionIds);
        }

        public List<Edition> GetEditionsByBook(int bookId)
        {
            return _editionRepository.FindByBook(bookId);
        }

        public List<Edition> GetEditionsByAuthor(int authorId)
        {
            return _editionRepository.FindByAuthor(authorId);
        }

        public List<Edition> SetMonitored(Edition edition)
        {
            return _editionRepository.SetMonitored(edition);
        }

        public void Handle(BookDeletedEvent message)
        {
            var editions = GetEditionsByBook(message.Book.Id);
            DeleteMany(editions);
        }
    }
}
