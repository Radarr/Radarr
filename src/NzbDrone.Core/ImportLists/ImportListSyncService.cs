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
using NzbDrone.Core.Tags;

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
        private readonly IImportListExclusionService _listExclusionService;
        private readonly IImportListMovieService _listMovieService;
        private readonly ITagService _tagService;

        public ImportListSyncService(IImportListFactory importListFactory,
                                      IFetchAndParseImportList listFetcherAndParser,
                                      IMovieService movieService,
                                      IAddMovieService addMovieService,
                                      IConfigService configService,
                                      IImportListExclusionService listExclusionService,
                                      IImportListMovieService listMovieService,
                                      ITagService tagService,
                                      Logger logger)
        {
            _importListFactory = importListFactory;
            _listFetcherAndParser = listFetcherAndParser;
            _movieService = movieService;
            _addMovieService = addMovieService;
            _listExclusionService = listExclusionService;
            _listMovieService = listMovieService;
            _tagService = tagService;
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

        private void ProcessMovieReport(ImportListDefinition importList, ImportListMovie report, List<ImportListExclusion> listExclusions, HashSet<int> dbMovies, Dictionary<int, Movie> moviesToAdd, Dictionary<int, HashSet<int>> existingMovieTagUpdates, Dictionary<int, string> allTags)
        {
            if (report.TmdbId == 0 || !importList.EnableAuto)
            {
                return;
            }

            // Check to see if movie in DB
            if (dbMovies.Contains(report.TmdbId))
            {
                _logger.Debug("{0} [{1}] Movie Exists in DB, checking tags", report.TmdbId, report.Title);

                // Collect tags to add to existing movies
                if (importList.Tags.Any())
                {
                    if (!existingMovieTagUpdates.TryGetValue(report.TmdbId, out var tagsToAdd))
                    {
                        tagsToAdd = new HashSet<int>();
                        existingMovieTagUpdates[report.TmdbId] = tagsToAdd;
                    }

                    tagsToAdd.UnionWith(importList.Tags);
                }

                return;
            }

            // Check to see if movie excluded
            var excludedMovie = listExclusions.SingleOrDefault(s => s.TmdbId == report.TmdbId);

            if (excludedMovie != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exclusion", report.TmdbId, report.Title);
                return;
            }

            // Check if movie is already on the add list (from another import list)
            if (moviesToAdd.TryGetValue(report.TmdbId, out var existingMovie))
            {
                // Merge tags from this list into the existing movie
                if (importList.Tags.Any())
                {
                    var newTags = importList.Tags.Except(existingMovie.Tags).ToList();

                    if (newTags.Any())
                    {
                        var tagNames = newTags.Select(id => allTags.TryGetValue(id, out var name) ? name : id.ToString());
                        _logger.Debug("{0} [{1}] Merging {2} tags from list {3}: [{4}]", report.TmdbId, report.Title, newTags.Count, importList.Name, string.Join(", ", tagNames));
                        existingMovie.Tags = existingMovie.Tags.Union(importList.Tags).ToHashSet();
                    }
                }

                return;
            }

            var monitorType = importList.Monitor;

            var initialTagNames = importList.Tags.Select(id => allTags.TryGetValue(id, out var name) ? name : id.ToString());
            _logger.Debug("{0} [{1}] Adding to import queue from list {2} with tags: [{3}]", report.TmdbId, report.Title, importList.Name, string.Join(", ", initialTagNames));

            moviesToAdd[report.TmdbId] = new Movie
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
            };
        }

        private void ProcessListItems(ImportListFetchResult listFetchResult)
        {
            var listedMovies = listFetchResult.Movies.ToList();

            var importExclusions = _listExclusionService.All();
            var dbMovies = _movieService.AllMovieTmdbIds().ToHashSet();
            var moviesToAdd = new Dictionary<int, Movie>();
            var existingMovieTagUpdates = new Dictionary<int, HashSet<int>>();
            var allTags = _tagService.All().ToDictionary(t => t.Id, t => t.Label);

            var groupedMovies = listedMovies.GroupBy(x => x.ListId);

            foreach (var list in groupedMovies)
            {
                var importList = _importListFactory.Get(list.Key);

                foreach (var movie in list)
                {
                    if (movie.TmdbId != 0)
                    {
                        ProcessMovieReport(importList, movie, importExclusions, dbMovies, moviesToAdd, existingMovieTagUpdates, allTags);
                    }
                }
            }

            if (moviesToAdd.Any())
            {
                _logger.ProgressInfo("Adding {0} movies from your auto enabled lists to library", moviesToAdd.Count);
                _addMovieService.AddMovies(moviesToAdd.Values.ToList(), true);
            }

            if (existingMovieTagUpdates.Any())
            {
                UpdateTagsForExistingMovies(existingMovieTagUpdates, allTags);
            }
        }

        private void UpdateTagsForExistingMovies(Dictionary<int, HashSet<int>> existingMovieTagUpdates, Dictionary<int, string> allTags)
        {
            var moviesToUpdate = new List<Movie>();
            var existingMovies = _movieService.FindByTmdbId(existingMovieTagUpdates.Keys.ToList());

            foreach (var movie in existingMovies)
            {
                if (existingMovieTagUpdates.TryGetValue(movie.TmdbId, out var tagsFromLists))
                {
                    var newTags = tagsFromLists.Except(movie.Tags).ToList();

                    if (newTags.Any())
                    {
                        movie.Tags = movie.Tags.Union(tagsFromLists).ToHashSet();
                        moviesToUpdate.Add(movie);

                        var tagNames = newTags.Select(id => allTags.TryGetValue(id, out var name) ? name : id.ToString());
                        _logger.Debug("Adding {0} tags to {1}: {2}", newTags.Count, movie.Title, string.Join(", ", tagNames));
                    }
                }
            }

            if (moviesToUpdate.Any())
            {
                _logger.ProgressInfo("Updating tags for {0} movies from import lists", moviesToUpdate.Count);
                _movieService.UpdateMovie(moviesToUpdate, true);
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
                var movieExists = listMovies.Any(c =>
                    c.TmdbId == movie.TmdbId ||
                    (c.ImdbId.IsNotNullOrWhiteSpace() && movie.ImdbId.IsNotNullOrWhiteSpace() && c.ImdbId == movie.ImdbId));

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
