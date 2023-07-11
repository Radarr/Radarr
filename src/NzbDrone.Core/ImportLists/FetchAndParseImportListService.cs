using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.ImportLists
{
    public interface IFetchAndParseImportList
    {
        ImportListFetchResult Fetch();
        ImportListFetchResult FetchSingleList(ImportListDefinition definition);
    }

    public class FetchAndParseImportListService : IFetchAndParseImportList
    {
        private readonly IImportListFactory _importListFactory;
        private readonly IImportListStatusService _importListStatusService;
        private readonly IImportListMovieService _listMovieService;
        private readonly ISearchForNewMovie _movieSearch;
        private readonly IProvideMovieInfo _movieInfoService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly Logger _logger;

        public FetchAndParseImportListService(IImportListFactory importListFactory,
                                              IImportListStatusService importListStatusService,
                                              IImportListMovieService listMovieService,
                                              ISearchForNewMovie movieSearch,
                                              IProvideMovieInfo movieInfoService,
                                              IMovieMetadataService movieMetadataService,
                                              Logger logger)
        {
            _importListFactory = importListFactory;
            _importListStatusService = importListStatusService;
            _listMovieService = listMovieService;
            _movieSearch = movieSearch;
            _movieInfoService = movieInfoService;
            _movieMetadataService = movieMetadataService;
            _logger = logger;
        }

        public ImportListFetchResult Fetch()
        {
            var result = new ImportListFetchResult();

            var importLists = _importListFactory.Enabled();

            if (!importLists.Any())
            {
                _logger.Debug("No available import lists. check your configuration.");
                return result;
            }

            _logger.Debug("Available import lists {0}", importLists.Count);

            var taskList = new List<Task>();
            var taskFactory = new TaskFactory(TaskCreationOptions.LongRunning, TaskContinuationOptions.None);

            foreach (var importList in importLists)
            {
                var importListLocal = importList;
                var importListStatus = _importListStatusService.GetLastSyncListInfo(importListLocal.Definition.Id);

                if (importListStatus.HasValue)
                {
                    var importListNextSync = importListStatus.Value + importListLocal.MinRefreshInterval;

                    if (DateTime.UtcNow < importListNextSync)
                    {
                        _logger.Trace("Skipping refresh of Import List {0} ({1}) due to minimum refresh interval. Next Sync after {2}", importList.Name, importListLocal.Definition.Name, importListNextSync);

                        continue;
                    }
                }

                _logger.ProgressInfo("Syncing Movies for Import List {0} ({1})", importList.Name, importListLocal.Definition.Name);

                var blockedLists = _importListStatusService.GetBlockedProviders().ToDictionary(v => v.ProviderId, v => v);

                if (blockedLists.TryGetValue(importList.Definition.Id, out var blockedListStatus))
                {
                    _logger.Debug("Temporarily ignoring Import List {0} ({1}) till {2} due to recent failures.", importList.Name, importListLocal.Definition.Name, blockedListStatus.DisabledTill.Value.ToLocalTime());
                    result.AnyFailure |= true; // Ensure we don't clean if a list is down
                    continue;
                }

                var task = taskFactory.StartNew(() =>
                {
                    try
                    {
                        var importListReports = importListLocal.Fetch();

                        lock (result)
                        {
                            _logger.Debug("Found {0} from Import List {1} ({2})", importListReports.Movies.Count, importList.Name, importListLocal.Definition.Name);

                            if (!importListReports.AnyFailure)
                            {
                                var alreadyMapped = result.Movies.Where(x => importListReports.Movies.Any(r => r.TmdbId == x.TmdbId));
                                var listMovies = MapMovieReports(importListReports.Movies.Where(x => result.Movies.All(r => r.TmdbId != x.TmdbId)).ToList()).Where(x => x.TmdbId > 0).ToList();

                                listMovies.AddRange(alreadyMapped);
                                listMovies = listMovies.DistinctBy(x => x.TmdbId).ToList();
                                listMovies.ForEach(m => m.ListId = importList.Definition.Id);

                                result.Movies.AddRange(listMovies);
                                _listMovieService.SyncMoviesForList(listMovies, importList.Definition.Id);
                            }

                            result.AnyFailure |= importListReports.AnyFailure;

                            _importListStatusService.UpdateListSyncStatus(importList.Definition.Id);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error during Import List Sync of {0} ({1})", importList.Name, importListLocal.Definition.Name);
                    }
                }).LogExceptions();

                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());

            result.Movies = result.Movies.DistinctBy(r => new { r.TmdbId, r.ImdbId, r.Title }).ToList();

            _logger.Debug("Found {0} total reports from {1} lists", result.Movies.Count, importLists.Count);

            return result;
        }

        public ImportListFetchResult FetchSingleList(ImportListDefinition definition)
        {
            var result = new ImportListFetchResult();

            var importList = _importListFactory.GetInstance(definition);

            if (importList == null || !definition.Enable)
            {
                _logger.Debug("Import List {0} ({1}) is not enabled, skipping.", importList.Name, importList.Definition.Name);
                return result;
            }

            var importListLocal = importList;

            try
            {
                var importListReports = importListLocal.Fetch();

                lock (result)
                {
                    _logger.Debug("Found {0} movies from {1} ({2})", importListReports.Movies.Count, importList.Name, importListLocal.Definition.Name);

                    if (!importListReports.AnyFailure)
                    {
                        var listMovies = MapMovieReports(importListReports.Movies).Where(x => x.TmdbId > 0).ToList();

                        listMovies = listMovies.DistinctBy(x => x.TmdbId).ToList();
                        listMovies.ForEach(m => m.ListId = importList.Definition.Id);

                        result.Movies.AddRange(listMovies);
                        _listMovieService.SyncMoviesForList(listMovies, importList.Definition.Id);
                    }

                    result.AnyFailure |= importListReports.AnyFailure;

                    _importListStatusService.UpdateListSyncStatus(importList.Definition.Id);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error during Import List Sync of {0} ({1})", importList.Name, importListLocal.Definition.Name);
            }

            result.Movies = result.Movies.DistinctBy(r => new { r.TmdbId, r.ImdbId, r.Title }).ToList();

            _logger.Debug("Found {0} movies from {1} ({2})", result.Movies.Count, importList.Name, importListLocal.Definition.Name);

            return result;
        }

        private List<ImportListMovie> MapMovieReports(List<ImportListMovie> reports)
        {
            var mappedMovies = reports.Select(m => _movieSearch.MapMovieToTmdbMovie(new MovieMetadata { Title = m.Title, TmdbId = m.TmdbId, ImdbId = m.ImdbId, Year = m.Year }))
                .Where(x => x != null)
                .DistinctBy(x => x.TmdbId)
                .ToList();

            _movieMetadataService.UpsertMany(mappedMovies);

            var mappedListMovies = new List<ImportListMovie>();

            foreach (var movieMeta in mappedMovies)
            {
                var mappedListMovie = new ImportListMovie();

                if (movieMeta != null)
                {
                    mappedListMovie.MovieMetadata = movieMeta;
                    mappedListMovie.MovieMetadataId = movieMeta.Id;
                }

                mappedListMovies.Add(mappedListMovie);
            }

            return mappedListMovies;
        }
    }
}
