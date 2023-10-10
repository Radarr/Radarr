using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.ImportLists.ImportExclusions;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListSyncService : IExecute<ImportListSyncCommand>
    {
        private readonly Logger _logger;
        private readonly IImportListFactory _importListFactory;
        private readonly IFetchAndParseImportList _listFetcherAndParser;
        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly IConfigService _configService;
        private readonly IImportExclusionsService _exclusionService;
        private readonly IImportListMovieService _listMovieService;

        public ImportListSyncService(IImportListFactory importListFactory,
                                      IFetchAndParseImportList listFetcherAndParser,
                                      IMovieService movieService,
                                      IAddMovieService addMovieService,
                                      IConfigService configService,
                                      IImportExclusionsService exclusionService,
                                      IImportListMovieService listMovieService,
                                      Logger logger)
        {
            _importListFactory = importListFactory;
            _listFetcherAndParser = listFetcherAndParser;
            _movieService = movieService;
            _addMovieService = addMovieService;
            _exclusionService = exclusionService;
            _listMovieService = listMovieService;
            _logger = logger;
            _configService = configService;
        }

        private void SyncAll()
        {
            if (_importListFactory.Enabled().Empty())
            {
                _logger.Debug("No enabled import lists, skipping sync and cleaning");

                return;
            }

            var listItemsResult = _listFetcherAndParser.Fetch();

            if (listItemsResult.SyncedLists == 0)
            {
                return;
            }

            if (!listItemsResult.AnyFailure)
            {
                CleanLibrary();
            }

            ProcessListItems(listItemsResult);
        }

        private void SyncList(ImportListDefinition definition)
        {
            _logger.ProgressInfo("Starting Import List Refresh for List {0}", definition.Name);

            var listItemsResult = _listFetcherAndParser.FetchSingleList(definition);

            ProcessListItems(listItemsResult);
        }

        private void ProcessMovieReport(ImportListDefinition importList, ImportListMovie report, List<ImportExclusion> listExclusions, List<int> dbMovies, List<Movie> moviesToAdd)
        {
            if (report.TmdbId == 0 || !importList.EnableAuto)
            {
                return;
            }

            // Check to see if movie in DB
            if (dbMovies.Contains(report.TmdbId))
            {
                _logger.Debug("{0} [{1}] Rejected, Movie Exists in DB", report.TmdbId, report.Title);
                return;
            }

            // Check to see if movie excluded
            var excludedMovie = listExclusions.SingleOrDefault(s => s.TmdbId == report.TmdbId);

            if (excludedMovie != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.TmdbId, report.Title);
                return;
            }

            // Append Artist if not already in DB or already on add list
            if (moviesToAdd.All(s => s.TmdbId != report.TmdbId))
            {
                var monitorType = importList.Monitor;

                moviesToAdd.Add(new Movie
                {
                    Monitored = monitorType != MonitorTypes.None,
                    RootFolderPath = importList.RootFolderPath,
                    QualityProfileId = importList.QualityProfileId,
                    MinimumAvailability = importList.MinimumAvailability,
                    Tags = importList.Tags,
                    TmdbId = report.TmdbId,
                    Title = report.Title,
                    Year = report.Year,
                    ImdbId = report.ImdbId,
                    AddOptions = new AddMovieOptions
                    {
                        SearchForMovie = monitorType != MonitorTypes.None && importList.SearchOnAdd,
                        Monitor = monitorType,
                        AddMethod = AddMovieMethod.List
                    }
                });
            }
        }

        private void ProcessListItems(ImportListFetchResult listFetchResult)
        {
            listFetchResult.Movies = listFetchResult.Movies.DistinctBy(x =>
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
            }).ToList();

            var listedMovies = listFetchResult.Movies.ToList();

            var importExclusions = _exclusionService.GetAllExclusions();
            var dbMovies = _movieService.AllMovieTmdbIds();
            var moviesToAdd = new List<Movie>();

            var groupedMovies = listedMovies.GroupBy(x => x.ListId);

            foreach (var list in groupedMovies)
            {
                var importList = _importListFactory.Get(list.Key);

                foreach (var movie in list)
                {
                    if (movie.TmdbId != 0)
                    {
                        ProcessMovieReport(importList, movie, importExclusions, dbMovies, moviesToAdd);
                    }
                }
            }

            if (moviesToAdd.Any())
            {
                _logger.ProgressInfo("Adding {0} movies from your auto enabled lists to library", moviesToAdd.Count);
                _addMovieService.AddMovies(moviesToAdd, true);
            }
        }

        public void Execute(ImportListSyncCommand message)
        {
            if (message.DefinitionId.HasValue)
            {
                SyncList(_importListFactory.Get(message.DefinitionId.Value));
            }
            else
            {
                SyncAll();
            }
        }

        private void CleanLibrary()
        {
            if (_configService.ListSyncLevel == "disabled")
            {
                return;
            }

            var listMovies = _listMovieService.GetAllListMovies();

            // TODO use AllMovieTmdbIds here?
            var moviesInLibrary = _movieService.GetAllMovies();

            var moviesToUpdate = new List<Movie>();

            foreach (var movie in moviesInLibrary)
            {
                var movieExists = listMovies.Any(c => c.TmdbId == movie.TmdbId || c.ImdbId == movie.ImdbId);

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
                            break;
                    }
                }
            }

            _movieService.UpdateMovie(moviesToUpdate, true);
        }
    }
}
