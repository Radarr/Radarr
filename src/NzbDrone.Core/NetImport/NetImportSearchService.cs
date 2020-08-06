using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies;
using NzbDrone.Core.NetImport.ImportExclusions;
using NzbDrone.Core.NetImport.ListMovies;

namespace NzbDrone.Core.NetImport
{
    public interface IFetchNetImport
    {
    }

    public class NetImportSearchService : IFetchNetImport, IExecute<NetImportSyncCommand>
    {
        private readonly Logger _logger;
        private readonly INetImportFactory _netImportFactory;
        private readonly IMovieService _movieService;
        private readonly IAddMovieService _addMovieService;
        private readonly IListMovieService _listMovieService;
        private readonly ISearchForNewMovie _movieSearch;
        private readonly IConfigService _configService;
        private readonly IImportExclusionsService _exclusionService;

        public NetImportSearchService(INetImportFactory netImportFactory,
                                      IMovieService movieService,
                                      IAddMovieService addMovieService,
                                      IListMovieService listMovieService,
                                      ISearchForNewMovie movieSearch,
                                      IConfigService configService,
                                      IImportExclusionsService exclusionService,
                                      Logger logger)
        {
            _netImportFactory = netImportFactory;
            _movieService = movieService;
            _addMovieService = addMovieService;
            _listMovieService = listMovieService;
            _movieSearch = movieSearch;
            _exclusionService = exclusionService;
            _logger = logger;
            _configService = configService;
        }

        private NetImportFetchResult GetListMovies()
        {
            var movies = new List<ListMovie>();
            var anyFailure = false;

            var importLists = _netImportFactory.GetAvailableProviders();

            foreach (var list in importLists)
            {
                var result = list.Fetch();

                if (!result.AnyFailure)
                {
                    // TODO some opportunity to bulk map here if we had the tmdbIds
                    result.Movies.ToList().ForEach(x =>
                    {
                        // TODO some logic to avoid mapping everything (if its a tmdb in the db use the existing movie, etc..)
                        MapMovieReport(x);
                    });

                    movies.AddRange(result.Movies);
                    _listMovieService.SyncMoviesForList(result.Movies.ToList(), list.Definition.Id);
                }

                anyFailure |= result.AnyFailure;
            }

            _logger.Debug("Found {0} movies from list(s) {1}", movies.Count, string.Join(", ", importLists.Select(l => l.Definition.Name)));

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

        private void ProcessMovieReport(NetImportDefinition importList, ListMovie report, List<ImportExclusion> listExclusions, List<Movie> moviesToAdd)
        {
            if (report.TmdbId == 0 || !importList.EnableAuto)
            {
                return;
            }

            // Check to see if movie in DB
            var existingMovie = _movieService.FindByTmdbId(report.TmdbId);

            if (existingMovie != null)
            {
                _logger.Debug("{0} [{1}] Rejected, Movie Exists in DB", report.TmdbId, report.Title);
                return;
            }

            // Check to see if movie excluded
            var excludedMovie = listExclusions.Where(s => s.TmdbId == report.TmdbId).SingleOrDefault();

            if (excludedMovie != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.TmdbId, report.Title);
                return;
            }

            // Append Artist if not already in DB or already on add list
            if (moviesToAdd.All(s => s.TmdbId != report.TmdbId))
            {
                var monitored = importList.ShouldMonitor;

                moviesToAdd.Add(new Movie
                {
                    Monitored = monitored,
                    RootFolderPath = importList.RootFolderPath,
                    ProfileId = importList.ProfileId,
                    MinimumAvailability = importList.MinimumAvailability,
                    Tags = importList.Tags,
                    AddOptions = new AddMovieOptions
                    {
                        SearchForMovie = monitored,
                    }
                });
            }
        }

        private void SyncAll()
        {
            var result = GetListMovies();

            //if there are no lists that are enabled for automatic import then dont do anything
            if (_netImportFactory.GetAvailableProviders().Where(a => ((NetImportDefinition)a.Definition).EnableAuto).Empty())
            {
                _logger.Info("No lists are enabled for auto-import.");
                return;
            }

            var listedMovies = result.Movies.ToList();

            if (!result.AnyFailure)
            {
                CleanLibrary(listedMovies);
            }

            var importExclusions = _exclusionService.GetAllExclusions();
            var moviesToAdd = new List<Movie>();

            foreach (var movie in listedMovies)
            {
                var importList = _netImportFactory.Get(movie.ListId);

                if (movie.TmdbId != 0)
                {
                    ProcessMovieReport(importList, movie, importExclusions, moviesToAdd);
                }
            }

            if (moviesToAdd.Any())
            {
                _logger.Info($"Adding {moviesToAdd.Count()} movies from your auto enabled lists to library");
            }

            _addMovieService.AddMovies(moviesToAdd, true);
        }

        private void MapMovieReport(ListMovie report)
        {
            var mappedMovie = _movieSearch.MapMovieToTmdbMovie(new Movie { Title = report.Title, TmdbId = report.TmdbId, ImdbId = report.ImdbId, Year = report.Year });

            report.TmdbId = mappedMovie?.TmdbId ?? 0;
            report.ImdbId = mappedMovie?.ImdbId;
            report.Title = mappedMovie?.Title;
            report.SortTitle = mappedMovie?.SortTitle;
            report.Year = mappedMovie?.Year ?? 0;
            report.Overview = mappedMovie?.Overview;
            report.Ratings = mappedMovie?.Ratings;
            report.Studio = mappedMovie?.Studio;
            report.Certification = mappedMovie.Certification;
            report.Collection = mappedMovie.Collection;
            report.Status = mappedMovie.Status;
            report.Images = mappedMovie?.Images;
            report.Website = mappedMovie?.Website;
            report.YouTubeTrailerId = mappedMovie?.YouTubeTrailerId;
            report.Translations = mappedMovie?.Translations;
            report.InCinemas = mappedMovie?.InCinemas;
            report.PhysicalRelease = mappedMovie?.PhysicalRelease;
            report.DigitalRelease = mappedMovie?.DigitalRelease;
            report.Genres = mappedMovie?.Genres;
        }

        public void Execute(NetImportSyncCommand message)
        {
            SyncAll();
        }

        private void CleanLibrary(List<ListMovie> listMovies)
        {
            var moviesToUpdate = new List<Movie>();

            if (_configService.ListSyncLevel == "disabled")
            {
                return;
            }

            var moviesInLibrary = _movieService.GetAllMovies();
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

                            //TODO: for some reason the files are not deleted in this case... any idea why?
                            break;
                        default:
                            break;
                    }
                }
            }

            _movieService.UpdateMovie(moviesToUpdate, true);
        }
    }
}
