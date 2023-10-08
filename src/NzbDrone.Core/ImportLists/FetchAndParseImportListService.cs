using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;

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
        private readonly Logger _logger;

        public FetchAndParseImportListService(IImportListFactory importListFactory,
                                              IImportListStatusService importListStatusService,
                                              Logger logger)
        {
            _importListFactory = importListFactory;
            _importListStatusService = importListStatusService;
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
                                var listMovies = importListReports.Movies;

                                listMovies.ForEach(m => m.ListId = importList.Definition.Id);

                                result.Movies.AddRange(listMovies);

                                result.SyncedWithoutFailure.Add(importList.Definition.Id);
                            }

                            result.AnyFailure |= importListReports.AnyFailure;
                            result.SyncedLists.Add(importList.Definition.Id);
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

            foreach (var list in importLists)
            {
                if (result.SyncedLists.Contains(list.Definition.Id))
                {
                    _importListStatusService.UpdateListSyncStatus(list.Definition.Id);
                }
            }

            result.Movies = result.Movies.DistinctBy(r => new { r.TmdbId, r.ImdbId, r.Title }).ToList();

            _logger.Debug("Found {0} total reports from {1} lists", result.Movies.Count, result.SyncedLists.Count);

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
                        var listMovies = importListReports.Movies;

                        listMovies.ForEach(m => m.ListId = importList.Definition.Id);

                        result.Movies.AddRange(listMovies);

                        result.SyncedWithoutFailure.Add(importList.Definition.Id);
                    }

                    result.AnyFailure |= importListReports.AnyFailure;

                    result.SyncedLists.Add(importList.Definition.Id);

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
    }
}
