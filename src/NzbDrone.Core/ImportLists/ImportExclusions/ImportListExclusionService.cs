using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.ImportLists.ImportExclusions
{
    public interface IImportListExclusionService
    {
        ImportListExclusion Add(ImportListExclusion importListExclusion);
        List<ImportListExclusion> Add(List<ImportListExclusion> importListExclusions);
        List<ImportListExclusion> All();
        PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec);
        bool IsMovieExcluded(int tmdbId);
        void Delete(int id);
        void Delete(List<int> ids);
        ImportListExclusion Get(int id);
        ImportListExclusion Update(ImportListExclusion importListExclusion);
        List<int> AllExcludedTmdbIds();
    }

    public class ImportListExclusionService : IImportListExclusionService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IImportListExclusionRepository _repo;
        private readonly Logger _logger;

        public ImportListExclusionService(IImportListExclusionRepository repo,
                             Logger logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public ImportListExclusion Add(ImportListExclusion importListExclusion)
        {
            if (_repo.IsMovieExcluded(importListExclusion.TmdbId))
            {
                return _repo.FindByTmdbid(importListExclusion.TmdbId);
            }

            return _repo.Insert(importListExclusion);
        }

        public List<ImportListExclusion> Add(List<ImportListExclusion> importListExclusions)
        {
            _repo.InsertMany(DeDupeExclusions(importListExclusions));

            return importListExclusions;
        }

        public bool IsMovieExcluded(int tmdbId)
        {
            return _repo.IsMovieExcluded(tmdbId);
        }

        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        public void Delete(List<int> ids)
        {
            _repo.DeleteMany(ids);
        }

        public ImportListExclusion Get(int id)
        {
            return _repo.Get(id);
        }

        public ImportListExclusion Update(ImportListExclusion importListExclusion)
        {
            return _repo.Update(importListExclusion);
        }

        public List<ImportListExclusion> All()
        {
            return _repo.All().ToList();
        }

        public PagingSpec<ImportListExclusion> Paged(PagingSpec<ImportListExclusion> pagingSpec)
        {
            return _repo.GetPaged(pagingSpec);
        }

        public List<int> AllExcludedTmdbIds()
        {
            return _repo.AllExcludedTmdbIds();
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            if (!message.AddImportListExclusion)
            {
                return;
            }

            _logger.Debug("Adding {0} deleted movies to import list exclusions.", message.Movies.Count);

            var exclusionsToAdd = DeDupeExclusions(message.Movies.Select(m => new ImportListExclusion
            {
                TmdbId = m.TmdbId,
                MovieTitle = m.Title,
                MovieYear = m.Year
            }).ToList());

            _repo.InsertMany(exclusionsToAdd);
        }

        private List<ImportListExclusion> DeDupeExclusions(List<ImportListExclusion> exclusions)
        {
            var existingExclusions = _repo.AllExcludedTmdbIds();

            return exclusions
                .DistinctBy(x => x.TmdbId)
                .Where(x => !existingExclusions.Contains(x.TmdbId))
                .ToList();
        }
    }
}
