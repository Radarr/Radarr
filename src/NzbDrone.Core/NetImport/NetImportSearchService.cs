using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.ImportExclusions;

namespace NzbDrone.Core.NetImport
{
    public interface IFetchNetImport
    {
        NetImportFetchResult Fetch(int listId, bool onlyEnableAuto);
        List<Movie> FetchAndFilter(int listId, bool onlyEnableAuto);
    }

    public class NetImportSearchService : IFetchNetImport, IExecute<NetImportSyncCommand>
    {
        private readonly Logger _logger;
        private readonly INetImportFactory _netImportFactory;
        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly ISearchForNewMovie _movieSearch;
        private readonly IConfigService _configService;
        private readonly IImportExclusionsService _exclusionService;

        public NetImportSearchService(INetImportFactory netImportFactory,
                                      IMovieService movieService,
                                      IAddMovieService addMovieService,
                                      ISearchForNewMovie movieSearch,
                                      IConfigService configService,
                                      IImportExclusionsService exclusionService,
                                      Logger logger)
        {
            _netImportFactory = netImportFactory;
            _movieService = movieService;
            _addMovieService = addMovieService;
            _movieSearch = movieSearch;
            _exclusionService = exclusionService;
            _logger = logger;
            _configService = configService;
        }

        public NetImportFetchResult Fetch(int listId, bool onlyEnableAuto = false)
        {
            return MovieListSearch(listId, onlyEnableAuto);
        }

        public List<Movie> FetchAndFilter(int listId, bool onlyEnableAuto)
        {
            var movies = MovieListSearch(listId, onlyEnableAuto).Movies;

            return _movieService.FilterExistingMovies(movies.ToList());
        }

        public NetImportFetchResult MovieListSearch(int listId, bool onlyEnableAuto = false)
        {
            var movies = new List<Movie>();
            var anyFailure = false;

            var importLists = _netImportFactory.GetAvailableProviders();

            var lists = listId == 0 ? importLists : importLists.Where(n => ((NetImportDefinition)n.Definition).Id == listId);

            if (onlyEnableAuto)
            {
                lists = importLists.Where(a => ((NetImportDefinition)a.Definition).EnableAuto);
            }

            foreach (var list in lists)
            {
                var result = list.Fetch();
                movies.AddRange(result.Movies);
                anyFailure |= result.AnyFailure;
            }

            _logger.Debug("Found {0} movies from list(s) {1}", movies.Count, string.Join(", ", lists.Select(l => l.Definition.Name)));

            return new NetImportFetchResult
            {
                Movies = movies.DistinctBy(x =>
                {
                    if (x.TmdbId != 0)
                    {
                        return x.TmdbId.ToString();
                    }

                    if (x.ImdbId.IsNotNullOrWhiteSpace())
                    {
                        return x.ImdbId;
                    }

                    return x.Title;
                }).ToList(),
                AnyFailure = anyFailure
            };
        }

        public void Execute(NetImportSyncCommand message)
        {
            //if there are no lists that are enabled for automatic import then dont do anything
            if (_netImportFactory.GetAvailableProviders().Where(a => ((NetImportDefinition)a.Definition).EnableAuto).Empty())
            {
                _logger.Info("No lists are enabled for auto-import.");
                return;
            }

            var result = Fetch(0, true);
            var listedMovies = result.Movies.ToList();

            if (!result.AnyFailure)
            {
                CleanLibrary(listedMovies);
            }

            listedMovies = listedMovies.Where(x => !_movieService.MovieExists(x)).ToList();
            if (listedMovies.Any())
            {
                _logger.Info($"Found {listedMovies.Count()} movies on your auto enabled lists not in your library");
            }

            var tasks = listedMovies.Select(x => _movieSearch.MapMovieToTmdbMovieAsync(x)).ToList();

            var importExclusions = new List<string>();
            var moviesToAdd = new List<Movie>();

            while (tasks.Count > 0)
            {
                var finishedTask = Task.WhenAny(tasks).GetAwaiter().GetResult();
                tasks.Remove(finishedTask);

                var mapped = finishedTask.GetAwaiter().GetResult();

                if (mapped != null && mapped.TmdbId > 0)
                {
                    if (_exclusionService.IsMovieExcluded(mapped.TmdbId))
                    {
                        _logger.Debug($"{mapped.Title} ({mapped.TitleSlug}) will not be added since it was found on the exclusions list");
                    }
                    else if (_movieService.MovieExists(mapped))
                    {
                        _logger.Trace($"{mapped.Title} ({mapped.TitleSlug}) will not be added since it exists in Library");
                    }
                    else
                    {
                        if (!moviesToAdd.Any(c => c.TmdbId == mapped.TmdbId))
                        {
                            mapped.AddOptions = new AddMovieOptions { SearchForMovie = true };
                            moviesToAdd.Add(mapped);
                        }
                    }
                }
            }

            if (moviesToAdd.Any())
            {
                _logger.Info($"Adding {moviesToAdd.Count()} movies from your auto enabled lists to library");
            }

            _addMovieService.AddMovies(moviesToAdd);
        }

        private void CleanLibrary(List<Movie> movies)
        {
            var moviesToUpdate = new List<Movie>();

            if (_configService.ListSyncLevel != "disabled")
            {
                var moviesInLibrary = _movieService.GetAllMovies();
                foreach (var movie in moviesInLibrary)
                {
                    var movieExists = movies.Any(c => c.TmdbId == movie.TmdbId || c.ImdbId == movie.ImdbId);

                    if (!movieExists)
                    {
                        switch (_configService.ListSyncLevel)
                        {
                            case "logOnly":
                                _logger.Info("{0} was in your library, but not found in your lists --> You might want to unmonitor or remove it", movie);
                                break;
                            case "keepAndUnmonitor":
                                _logger.Info("{0} was in your library, but not found in your lists --> Keeping in library but Unmonitoring it", movie);
                                movie.Monitored = false;
                                moviesToUpdate.Add(movie);
                                break;
                            case "removeAndKeep":
                                _logger.Info("{0} was in your library, but not found in your lists --> Removing from library (keeping files)", movie);
                                _movieService.DeleteMovie(movie.Id, false);
                                break;
                            case "removeAndDelete":
                                _logger.Info("{0} was in your library, but not found in your lists --> Removing from library and deleting files", movie);
                                _movieService.DeleteMovie(movie.Id, true);

                                //TODO: for some reason the files are not deleted in this case... any idea why?
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            _movieService.UpdateMovie(moviesToUpdate, true);
        }
    }
}
