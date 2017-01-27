using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.NetImport
{
    public interface IFetchNetImport
    {
        List<Movie> Fetch(int listId, bool onlyEnableAuto);
        List<Movie> FetchAndFilter(int listId, bool onlyEnableAuto);
    }

    public class NetImportSearchService : IFetchNetImport, IExecute<NetImportSyncCommand>
    {
        private readonly Logger _logger;
        private readonly INetImportFactory _netImportFactory;
        private readonly IMovieService _movieService;
        private readonly ISearchForNewMovie _movieSearch;
        private readonly IRootFolderService _rootFolder;

        public NetImportSearchService(INetImportFactory netImportFactory, IMovieService movieService,
            ISearchForNewMovie movieSearch, IRootFolderService rootFolder, Logger logger)
        {
            _netImportFactory = netImportFactory;
            _movieService = movieService;
            _movieSearch = movieSearch;
            _rootFolder = rootFolder;
            _logger = logger;
        }


        public List<Movie> Fetch(int listId, bool onlyEnableAuto = false)
        {
            return MovieListSearch(listId, onlyEnableAuto);
        }

        public List<Movie> FetchAndFilter(int listId, bool onlyEnableAuto)
        {
            var movies = MovieListSearch(listId, onlyEnableAuto);

            return movies.Where(x => !_movieService.MovieExists(x)).ToList();
        }

        public List<Movie> MovieListSearch(int listId, bool onlyEnableAuto = false)
        {
            var movies = new List<Movie>();

            var importLists = _netImportFactory.GetAvailableProviders();

            var lists = listId == 0 ? importLists : importLists.Where(n => ((NetImportDefinition)n.Definition).Id == listId);

            if (onlyEnableAuto)
            {
                lists = importLists.Where(a => ((NetImportDefinition)a.Definition).EnableAuto);
            }

            foreach (var list in lists)
            {
                movies.AddRange(list.Fetch());
            }

            _logger.Debug("Found {0} movies from list(s) {1}", movies.Count, string.Join(", ", lists.Select(l => l.Definition.Name)));

            return movies;
        }

        public void Execute(NetImportSyncCommand message)
        {
            var movies = FetchAndFilter(0, true);

            _logger.Debug("Found {0} movies on your auto enabled lists not in your library", movies.Count);

            foreach (var movie in movies)
            {
                var mapped = _movieSearch.MapMovieToTmdbMovie(movie);

                if (mapped != null)
                {
                    _movieService.AddMovie(mapped);
                }
            }
        }
    }
}
