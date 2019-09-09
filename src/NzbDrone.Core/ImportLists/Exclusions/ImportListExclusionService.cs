using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.ImportLists.Exclusions
{
    public interface IImportListExclusionService
    {
        ImportListExclusion Add(ImportListExclusion importListExclusion);
        List<ImportListExclusion> All();
        void Delete(int id);
        ImportListExclusion Get(int id);
        ImportListExclusion FindByForeignId(string foreignId);
        ImportListExclusion Update(ImportListExclusion importListExclusion);
    }

    public class ImportListExclusionService : IImportListExclusionService, IHandleAsync<ArtistDeletedEvent>
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

        public ImportListExclusion Get(int id)
        {
            return _repo.Get(id);
        }

        public ImportListExclusion FindByForeignId(string foreignId)
        {
            return _repo.FindByForeignId(foreignId);
        }

        public List<ImportListExclusion> All()
        {
            return _repo.All().ToList();
        }

        public void HandleAsync(ArtistDeletedEvent message)
        {
            if (!message.AddImportListExclusion)
            {
                return;
            }

            var existingExclusion = _repo.FindByForeignId(message.Artist.ForeignArtistId);

            if (existingExclusion != null)
            {
                return;
            }

            var importExclusion = new ImportListExclusion
            {
                ForeignId = message.Artist.ForeignArtistId,
                Name = message.Artist.Name
            };

            _repo.Insert(importExclusion);
        }
    }
}
