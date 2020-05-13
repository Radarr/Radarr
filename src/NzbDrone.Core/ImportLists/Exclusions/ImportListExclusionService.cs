using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.ImportLists.Exclusions
{
    public interface IImportListExclusionService
    {
        ImportListExclusion Add(ImportListExclusion importListExclusion);
        List<ImportListExclusion> All();
        void Delete(int id);
        void Delete(string foreignId);
        ImportListExclusion Get(int id);
        ImportListExclusion FindByForeignId(string foreignId);
        List<ImportListExclusion> FindByForeignId(List<string> foreignIds);
        ImportListExclusion Update(ImportListExclusion importListExclusion);
    }

    public class ImportListExclusionService : IImportListExclusionService,
                                              IHandleAsync<AuthorDeletedEvent>,
                                              IHandleAsync<BookDeletedEvent>
    {
        private readonly IImportListExclusionRepository _repo;

        public ImportListExclusionService(IImportListExclusionRepository repo)
        {
            _repo = repo;
        }

        public ImportListExclusion Add(ImportListExclusion importListExclusion)
        {
            return _repo.Insert(importListExclusion);
        }

        public ImportListExclusion Update(ImportListExclusion importListExclusion)
        {
            return _repo.Update(importListExclusion);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public void Delete(string foreignId)
        {
            var exclusion = FindByForeignId(foreignId);
            if (exclusion != null)
            {
                Delete(exclusion.Id);
            }
        }

        public ImportListExclusion Get(int id)
        {
            return _repo.Get(id);
        }

        public ImportListExclusion FindByForeignId(string foreignId)
        {
            return _repo.FindByForeignId(foreignId);
        }

        public List<ImportListExclusion> FindByForeignId(List<string> foreignIds)
        {
            return _repo.FindByForeignId(foreignIds);
        }

        public List<ImportListExclusion> All()
        {
            return _repo.All().ToList();
        }

        public void HandleAsync(AuthorDeletedEvent message)
        {
            if (!message.AddImportListExclusion)
            {
                return;
            }

            var existingExclusion = _repo.FindByForeignId(message.Author.ForeignAuthorId);

            if (existingExclusion != null)
            {
                return;
            }

            var importExclusion = new ImportListExclusion
            {
                ForeignId = message.Author.ForeignAuthorId,
                Name = message.Author.Name
            };

            _repo.Insert(importExclusion);
        }

        public void HandleAsync(BookDeletedEvent message)
        {
            if (!message.AddImportListExclusion)
            {
                return;
            }

            var existingExclusion = _repo.FindByForeignId(message.Book.ForeignBookId);

            if (existingExclusion != null)
            {
                return;
            }

            var importExclusion = new ImportListExclusion
            {
                ForeignId = message.Book.ForeignBookId,
                Name = $"{message.Book.AuthorMetadata.Value.Name} - {message.Book.Title}"
            };

            _repo.Insert(importExclusion);
        }
    }
}
