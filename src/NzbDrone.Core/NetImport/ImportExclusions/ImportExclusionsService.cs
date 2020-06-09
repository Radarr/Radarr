using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.NetImport.ImportExclusions
{
    public interface IImportExclusionsService
    {
        List<ImportExclusion> GetAllExclusions();
        bool IsMovieExcluded(int tmdbId);
        ImportExclusion AddExclusion(ImportExclusion exclusion);
        List<ImportExclusion> AddExclusions(List<ImportExclusion> exclusions);
        void RemoveExclusion(ImportExclusion exclusion);
        ImportExclusion GetById(int id);
    }

    public class ImportExclusionsService : IImportExclusionsService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly IImportExclusionsRepository _exclusionRepository;
        private readonly Logger _logger;

        public ImportExclusionsService(IImportExclusionsRepository exclusionRepository,
                             Logger logger)
        {
            _exclusionRepository = exclusionRepository;
            _logger = logger;
        }

        public ImportExclusion AddExclusion(ImportExclusion exclusion)
        {
            if (_exclusionRepository.IsMovieExcluded(exclusion.TmdbId))
            {
                return _exclusionRepository.GetByTmdbid(exclusion.TmdbId);
            }

            return _exclusionRepository.Insert(exclusion);
        }

        public List<ImportExclusion> AddExclusions(List<ImportExclusion> exclusions)
        {
            _exclusionRepository.InsertMany(exclusions);

            return exclusions;
        }

        public List<ImportExclusion> GetAllExclusions()
        {
            return _exclusionRepository.All().ToList();
        }

        public bool IsMovieExcluded(int tmdbId)
        {
            return _exclusionRepository.IsMovieExcluded(tmdbId);
        }

        public void RemoveExclusion(ImportExclusion exclusion)
        {
            _exclusionRepository.Delete(exclusion);
        }

        public ImportExclusion GetById(int id)
        {
            return _exclusionRepository.Get(id);
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            if (message.AddExclusion)
            {
                _logger.Debug("Adding {0} Deleted Movies to Net Import Exclusions", message.Movies.Count);
                _exclusionRepository.InsertMany(message.Movies.Select(m => new ImportExclusion { TmdbId = m.TmdbId, MovieTitle = m.Title, MovieYear = m.Year }).ToList());
            }
        }
    }
}
