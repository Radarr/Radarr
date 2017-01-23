using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport
{
    public interface IFetchNetImport
    {
        List<Movie> Fetch(int listId, bool onlyEnableAuto);
        List<Movie> FetchAndFilter(int listId, bool onlyEnableAuto);
    }

    public class NetImportSearchService : IFetchNetImport
    {
        private readonly Logger _logger;
        private readonly INetImportFactory _netImportFactory;
        private readonly IMovieService _movieService;

        public NetImportSearchService(INetImportFactory netImportFactory, IMovieService movieService, Logger logger)
        {
            _netImportFactory = netImportFactory;
            _movieService = movieService;
            _logger = logger;
        }

        public List<Movie> Fetch(int listId, bool onlyEnableAuto)
        {
            var movies = new List<Movie>();

            // Get all the lists
            var importLists = _netImportFactory.GetAvailableProviders();

            // No listId is set return all movies in all lists
            var lists = listId == 0 ? importLists.Where(n => ((NetImportDefinition) n.Definition).Enabled == true) : importLists.Where(n => ((NetImportDefinition) n.Definition).Id == listId);

            // Only return lists where enabledAuto is truthy
            if (onlyEnableAuto)
            {
                lists = importLists.Where(a => a.EnableAuto == true);
            }

            foreach (var list in lists)
            {
                movies.AddRange(list.Fetch());
            }

            return movies;
        }

        public List<Movie> FetchAndFilter(int listId, bool onlyEnableAuto)
        {
            var movies = new List<Movie>();

            // Get all the lists
            var importLists = _netImportFactory.GetAvailableProviders();

            // No listId is set return all movies in all lists
            var lists = listId == 0 ? importLists.Where(n => ((NetImportDefinition)n.Definition).Enabled == true) : importLists.Where(n => ((NetImportDefinition)n.Definition).Id == listId);

            // Only return lists where enabledAuto is truthy
            if (onlyEnableAuto)
            {
                lists = importLists.Where(a => a.EnableAuto == true);
            }

            // Get all existing movies
            var existingMovies = _movieService.GetAllMovies();

            foreach (var list in lists)
            {
                movies = (List<Movie>)list.Fetch();
            }

            // remove from movies list where existMovies (choose one)
            // movies.RemoveAll(x => existingMovies.Contains(x));
            // return movies;
            // movies.RemoveAll(a => existingMovies.Exists(w => w.TmdbId == a.TmdbId));
            // return movies;

            return movies.Where(x => !existingMovies.Contains(x)).ToList();
        }
    }
}
