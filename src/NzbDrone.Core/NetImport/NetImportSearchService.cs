using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Configuration;

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
        private readonly IConfigService _configService;
        

        public NetImportSearchService(INetImportFactory netImportFactory, IMovieService movieService,
            ISearchForNewMovie movieSearch, IRootFolderService rootFolder, IConfigService configService, Logger logger)
        {
            _netImportFactory = netImportFactory;
            _movieService = movieService;
            _movieSearch = movieSearch;
            _rootFolder = rootFolder;
            _logger = logger;
            _configService = configService;
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
            var listedMovies = Fetch(0, true);
            if (_configService.ListSyncLevel != "disabled")
            {
                var moviesInLibrary = _movieService.GetAllMovies();
                foreach (var movie in moviesInLibrary)
                    {
                    bool foundMatch = false;
                    foreach (var listedMovie in listedMovies)
                    {
                        if (movie.ImdbId == listedMovie.ImdbId)
                        {
                            foundMatch = true;
                            break;
                        }

                    }
                    if (!foundMatch)
                    {
                        if (_configService.ListSyncLevel == "logOnly")
                        {
                            _logger.Info("Movie: {0} was in your library, but not found in your lists --> you might want to unmonitor or remove it", movie.ImdbId);
                        }
                        else if (_configService.ListSyncLevel == "keepAndUnmonitor")
                        {
                            _logger.Info("Movie: {0} was in your library, but was not found in your lists --> Keeping in library but Unmonitoring it", movie.ImdbId);
                            movie.Monitored = false;
                        }
                        else if (_configService.ListSyncLevel == "removeAndKeep")
                        {
                            _logger.Info("Movie: {0} was in your library, but not found in your lists --> removing from library (keeping files)", movie.ImdbId);
                            _movieService.DeleteMovie(movie.Id, false); 
                        }
                        else if (_configService.ListSyncLevel == "removeAndDelete")
                        {
                            _logger.Info("Movie: {0} was in your library, but not found in your lists --> removing from library and deleting files", movie.ImdbId);
                            _movieService.DeleteMovie(movie.Id, true); 
                            //TODO: for some reason the files are not deleted in this case... any idea why? 
                        }
                    }
                }
            }

            List<string> importExclusions = null;
            if (_configService.ImportExclusions != String.Empty)
            {
                importExclusions = _configService.ImportExclusions.Split(',').ToList();
            }

            var movies = listedMovies.Where(x => !_movieService.MovieExists(x)).ToList();

            _logger.Debug("Found {0} movies on your auto enabled lists not in your library", movies.Count);

            foreach (var movie in movies)
            {
                bool shouldAdd = true;
                if (importExclusions != null)
                {
                    foreach (var exclusion in importExclusions)
                    {
                        if (exclusion == movie.ImdbId || exclusion == movie.TmdbId.ToString())
                        {
                            _logger.Info("Movie: {0} was found but will not be added because it {exclusion} was found on your exclusion list", exclusion);
                            shouldAdd = false;
                            break;
                        }
                    }
                }

                var mapped = _movieSearch.MapMovieToTmdbMovie(movie);
                if ((mapped != null) && shouldAdd)
                {
                    _movieService.AddMovie(mapped);
                }
            }
        }
    }
}
