using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Configuration;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch;
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
        private readonly ISearchForNewMovie _movieSearch;
        private readonly IRootFolderService _rootFolder;
        private readonly IConfigService _configService;
        private readonly ISearchForNzb _nzbSearchService;
        private readonly IProcessDownloadDecisions _processDownloadDecisions;
        private readonly IImportExclusionsService _exclusionService;


        public NetImportSearchService(INetImportFactory netImportFactory, IMovieService movieService,
            ISearchForNewMovie movieSearch, IRootFolderService rootFolder, ISearchForNzb nzbSearchService,
                                   IProcessDownloadDecisions processDownloadDecisions, IConfigService configService,
                                      IImportExclusionsService exclusionService,
                                      Logger logger)
        {
            _netImportFactory = netImportFactory;
            _movieService = movieService;
            _movieSearch = movieSearch;
            _nzbSearchService = nzbSearchService;
            _processDownloadDecisions = processDownloadDecisions;
            _rootFolder = rootFolder;
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
            if((_netImportFactory.GetAvailableProviders()).Where(a => ((NetImportDefinition)a.Definition).EnableAuto).Empty())
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


            var importExclusions = new List<string>();

            //var downloadedCount = 0;
            foreach (var movie in listedMovies)
            {

                var mapped = _movieSearch.MapMovieToTmdbMovie(movie);
                if (mapped != null && !_exclusionService.IsMovieExcluded(mapped.TmdbId))
                {
                    //List<DownloadDecision> decisions;
                    mapped.AddOptions = new AddMovieOptions {SearchForMovie = true};
                    _movieService.AddMovie(mapped);
                    //// Search for movie
                    //try
                    //{
                    //    decisions = _nzbSearchService.MovieSearch(mapped.Id, false);
                    //}
                    //catch (Exception ex)
                    //{
                    //    _logger.Error(ex, $"Unable to search in list for movie {mapped.Id}");
                    //    continue;
                    //}

                    //var processed = _processDownloadDecisions.ProcessDecisions(decisions);
                    //downloadedCount += processed.Grabbed.Count;

                }
                else
                {
                    if (mapped != null)
                    {
                        _logger.Info($"{mapped.Title} ({mapped.TitleSlug}) will not be added since it was found on the exclusions list");
                    }
                }
            }

            //_logger.ProgressInfo("Movie search completed. {0} reports downloaded.", downloadedCount);
        }

        private void CleanLibrary(List<Movie> movies)
        {
            var moviesToUpdate = new List<Movie>();
            if (_configService.ListSyncLevel != "disabled")
            {
                var moviesInLibrary = _movieService.GetAllMovies();
                foreach (var movie in moviesInLibrary)
                {
                    bool foundMatch = false;
                    foreach (var listedMovie in movies)
                    {
                        if (movie.TmdbId == listedMovie.TmdbId)
                        {
                            foundMatch = true;
                            switch (_configService.ListSyncLevel)
                            {
                                case "logOnly":
                                    _logger.Info("{0} was in your library and found in your lists --> You might want to unmonitor or remove it", movie);
                                    break;
                                case "keepAndUnmonitor":
                                    _logger.Info("{0} was in your library and found in your lists --> Keeping in library but Unmonitoring it", movie);
                                    movie.Monitored = false;
                                    break;
                                case "removeAndKeep":
                                    _logger.Info("{0} was in your library and found in your lists --> Removing from library (keeping files)", movie);
                                    _movieService.DeleteMovie(movie.Id, false, true);
                                    break;
                                case "removeAndDelete":
                                    _logger.Info("{0} was in your library and not found in your lists --> Removing from library and deleting files", movie);
                                    _movieService.DeleteMovie(movie.Id, true, true);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        }
                    }
                    if (!foundMatch)
                    {
                        _logger.Debug("movie {0} not found in your defined trakt list", movie.Title);

                    }
                    //wrong position
                    /*
                    if (!foundMatch)
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
                    */
                }
            }

            _movieService.UpdateMovie(moviesToUpdate);
        }
    }
}
