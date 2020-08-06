using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.ThingiProvider.Events;

namespace NzbDrone.Core.NetImport.ListMovies
{
    public interface IListMovieService
    {
        List<ListMovie> GetAllListMovies();
        List<ListMovie> GetAllForList(int listId);
        ListMovie AddListMovie(ListMovie listMovie);
        List<ListMovie> AddListMovies(List<ListMovie> listMovies);
        List<ListMovie> SyncMoviesForList(List<ListMovie> listMovies, int listId);
        void RemoveListMovie(ListMovie listMovie);
        ListMovie GetById(int id);
    }

    public class ListMovieService : IListMovieService, IHandleAsync<ProviderDeletedEvent<INetImport>>
    {
        private readonly IListMovieRepository _listMovieRepository;
        private readonly Logger _logger;

        public ListMovieService(IListMovieRepository listMovieRepository,
                             Logger logger)
        {
            _listMovieRepository = listMovieRepository;
            _logger = logger;
        }

        public ListMovie AddListMovie(ListMovie exclusion)
        {
            return _listMovieRepository.Insert(exclusion);
        }

        public List<ListMovie> AddListMovies(List<ListMovie> listMovies)
        {
            _listMovieRepository.InsertMany(listMovies);

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
        public List<ListMovie> SyncMoviesForList(List<ListMovie> listMovies, int listId)
        {
            var existingListMovies = GetAllForList(listId);

            listMovies.ForEach(l => l.Id = existingListMovies.FirstOrDefault(e => e.TmdbId == l.TmdbId)?.Id ?? 0);

            _listMovieRepository.InsertMany(listMovies.Where(l => l.Id == 0).ToList());
            _listMovieRepository.UpdateMany(listMovies.Where(l => l.Id > 0).ToList());
            _listMovieRepository.DeleteMany(existingListMovies.Where(l => !listMovies.Any(x => x.TmdbId == l.TmdbId)).ToList());

            return listMovies;
        }

        public List<ListMovie> GetAllListMovies()
        {
            return _listMovieRepository.All().ToList();
        }

        public List<ListMovie> GetAllForList(int listId)
        {
            return _listMovieRepository.GetAllForList(listId).ToList();
        }

        public void RemoveListMovie(ListMovie listMovie)
        {
            _listMovieRepository.Delete(listMovie);
        }

        public ListMovie GetById(int id)
        {
            return _listMovieRepository.Get(id);
        }

        public void HandleAsync(ProviderDeletedEvent<INetImport> message)
        {
            var moviesOnList = _listMovieRepository.GetAllForList(message.ProviderId);
            _listMovieRepository.DeleteMany(moviesOnList);
        }
    }
}
