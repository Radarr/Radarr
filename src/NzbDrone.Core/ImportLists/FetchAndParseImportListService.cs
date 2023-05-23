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
                _logger.ProgressInfo("Syncing Movies for List: {0}", importList.Name);

                var importListLocal = importList;
                var blockedLists = _importListStatusService.GetBlockedProviders().ToDictionary(v => v.ProviderId, v => v);

                if (blockedLists.TryGetValue(importList.Definition.Id, out var blockedListStatus))
                {
                    _logger.Debug("Temporarily ignoring list {0} till {1} due to recent failures.", importList.Definition.Name, blockedListStatus.DisabledTill.Value.ToLocalTime());
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
                            _logger.Debug("Found {0} from {1}", importListReports.Movies.Count, importList.Name);

                            if (!importListReports.AnyFailure)
                            {
                                var alreadyMapped = result.Movies.Where(x => importListReports.Movies.Any(r => r.TmdbId == x.TmdbId));
                                var listMovies = MapMovieReports(importListReports.Movies.Where(x => !result.Movies.Any(r => r.TmdbId == x.TmdbId)).ToList()).Where(x => x.TmdbId > 0).ToList();

                                listMovies.AddRange(alreadyMapped);
                                listMovies = listMovies.DistinctBy(x => x.TmdbId).ToList();
                                listMovies.ForEach(m => m.ListId = importList.Definition.Id);

                                result.Movies.AddRange(listMovies);
                                _listMovieService.SyncMoviesForList(listMovies, importList.Definition.Id);
                            }

                            result.AnyFailure |= importListReports.AnyFailure;
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Error during Import List Sync for list {0}", importList.Name);
                    }
                }).LogExceptions();

                taskList.Add(task);
            }

            Task.WaitAll(taskList.ToArray());

            _logger.Debug("Found {0} reports for all lists", result.Movies.Count);

            return result;
        }

        public ImportListFetchResult FetchSingleList(ImportListDefinition definition)
        {
            var result = new ImportListFetchResult();

            var importList = _importListFactory.GetInstance(definition);

            if (importList == null || !definition.Enable)
            {
                _logger.Debug("Import list {0} is not enabled. No Movies will be added");
                return result;
            }

            var importListLocal = importList;

            try
            {
                var importListReports = importListLocal.Fetch();

                lock (result)
                {
                    _logger.Debug("Found {0} from {1}", importListReports.Movies.Count, importList.Name);

                    if (!importListReports.AnyFailure)
                    {
                        var listMovies = MapMovieReports(importListReports.Movies).Where(x => x.TmdbId > 0).ToList();

                        listMovies = listMovies.DistinctBy(x => x.TmdbId).ToList();
                        listMovies.ForEach(m => m.ListId = importList.Definition.Id);

                        result.Movies.AddRange(listMovies);
                        _listMovieService.SyncMoviesForList(listMovies, importList.Definition.Id);
                    }

                    result.AnyFailure |= importListReports.AnyFailure;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error during Import List Sync for list {0}", importList.Name);
            }

            _logger.Debug("Found {0} reports for list {1}", result.Movies.Count, importList.Name);

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
