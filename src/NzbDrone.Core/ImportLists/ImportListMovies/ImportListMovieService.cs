using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.ImportLists.ImportListMovies
{
    public interface IImportListMovieService
    {
        List<ImportListMovie> GetAllListMovies();
        List<ImportListMovie> GetAllForList(int listId);
        ImportListMovie AddListMovie(ImportListMovie listMovie);
        List<ImportListMovie> AddListMovies(List<ImportListMovie> listMovies);
        List<ImportListMovie> SyncMoviesForList(List<ImportListMovie> listMovies, int listId);
        void RemoveListMovie(ImportListMovie listMovie);
        ImportListMovie GetById(int id);
    }

    public class ImportListMovieService : IImportListMovieService, IHandleAsync<ProviderDeletedEvent<IImportList>>
    {
        private readonly IImportListMovieRepository _importListMovieRepository;
        private readonly Logger _logger;

        public ImportListMovieService(IImportListMovieRepository importListMovieRepository,
                             Logger logger)
        {
            _importListMovieRepository = importListMovieRepository;
            _logger = logger;
        }

        public ImportListMovie AddListMovie(ImportListMovie exclusion)
        {
            return _importListMovieRepository.Insert(exclusion);
        }

        public List<ImportListMovie> AddListMovies(List<ImportListMovie> listMovies)
        {
            _importListMovieRepository.InsertMany(listMovies);

            return listMovies;
        }

        //public List<ListMovie> SyncMoviesForList(List<ListMovie> listMovies)
        //{
        //    var existingListMovies = GetAllListMovies();
        //    var newMovies = listMovies.GroupBy(x => x.TmdbId);

        //    listMovies = newMovies.Select(x =>
        //    {
        //        var movie = x.First();

        //        movie.ListIds = x.SelectMany(m => m.ListIds).ToList();

        //        return movie;
        //    }).ToList();

        //    listMovies.ForEach(l => l.Id = existingListMovies.FirstOrDefault(e => e.TmdbId == l.TmdbId)?.Id ?? 0);

        //    _listMovieRepository.InsertMany(listMovies.Where(l => l.Id == 0).ToList());
        //    _listMovieRepository.UpdateMany(listMovies.Where(l => l.Id > 0).ToList());
        //    _listMovieRepository.DeleteMany(existingListMovies.Where(l => !listMovies.Any(x => x.TmdbId == l.TmdbId)).ToList());

        //    return listMovies;
        //}
        public List<ImportListMovie> SyncMoviesForList(List<ImportListMovie> listMovies, int listId)
        {
            var existingListMovies = GetAllForList(listId);

            listMovies.ForEach(l => l.Id = existingListMovies.FirstOrDefault(e => e.TmdbId == l.TmdbId)?.Id ?? 0);

            _importListMovieRepository.InsertMany(listMovies.Where(l => l.Id == 0).ToList());
            _importListMovieRepository.UpdateMany(listMovies.Where(l => l.Id > 0).ToList());
            _importListMovieRepository.DeleteMany(existingListMovies.Where(l => !listMovies.Any(x => x.TmdbId == l.TmdbId)).ToList());

            return listMovies;
        }

        public List<ImportListMovie> GetAllListMovies()
        {
            return _importListMovieRepository.All().ToList();
        }

        public List<ImportListMovie> GetAllForList(int listId)
        {
            return _importListMovieRepository.GetAllForList(listId).ToList();
        }

        public void RemoveListMovie(ImportListMovie listMovie)
        {
            _importListMovieRepository.Delete(listMovie);
        }

        public ImportListMovie GetById(int id)
        {
            return _importListMovieRepository.Get(id);
        }

        public void HandleAsync(ProviderDeletedEvent<IImportList> message)
        {
            var moviesOnList = _importListMovieRepository.GetAllForList(message.ProviderId);
            _importListMovieRepository.DeleteMany(moviesOnList);
        }
    }
}
