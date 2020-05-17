using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies
{
    public class RefreshMovieService : IExecute<RefreshMovieCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IMovieService _movieService;
        private readonly IAlternativeTitleService _titleService;
        private readonly ICreditService _creditService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfMovieShouldBeRefreshed _checkIfMovieShouldBeRefreshed;
        private readonly IConfigService _configService;

        private readonly Logger _logger;

        public RefreshMovieService(IProvideMovieInfo movieInfo,
                                    IMovieService movieService,
                                    IAlternativeTitleService titleService,
                                    ICreditService creditService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    ICheckIfMovieShouldBeRefreshed checkIfMovieShouldBeRefreshed,
                                    IConfigService configService,
                                    Logger logger)
        {
            _movieInfo = movieInfo;
            _movieService = movieService;
            _titleService = titleService;
            _creditService = creditService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfMovieShouldBeRefreshed = checkIfMovieShouldBeRefreshed;
            _configService = configService;
            _logger = logger;
        }

        private void RefreshMovieInfo(Movie movie, Task<Tuple<Movie, List<Credit>>> dataTask)
        {
            _logger.ProgressInfo("Updating Info for {0}", movie.Title);

            Movie movieInfo;
            List<Credit> credits;

            try
            {
                var tuple = dataTask.GetAwaiter().GetResult();
                movieInfo = tuple.Item1;
                credits = tuple.Item2;
            }
            catch (MovieNotFoundException)
            {
                if (movie.Status != MovieStatusType.Deleted)
                {
                    movie.Status = MovieStatusType.Deleted;
                    _movieService.UpdateMovie(movie);
                    _logger.Debug("Movie marked as deleted on tmdb for {0}", movie.Title);
                    _eventAggregator.PublishEvent(new MovieUpdatedEvent(movie));
                }

                throw;
            }

            if (movie.TmdbId != movieInfo.TmdbId)
            {
                _logger.Warn("Movie '{0}' (Tmdbid {1}) was replaced with '{2}' (Tmdbid {3}), because the original was a duplicate.", movie.Title, movie.TmdbId, movieInfo.Title, movieInfo.TmdbId);
                movie.TmdbId = movieInfo.TmdbId;
            }

            movie.Title = movieInfo.Title;
            movie.TitleSlug = movieInfo.TitleSlug;
            movie.ImdbId = movieInfo.ImdbId;
            movie.Overview = movieInfo.Overview;
            movie.Status = movieInfo.Status;
            movie.CleanTitle = movieInfo.CleanTitle;
            movie.SortTitle = movieInfo.SortTitle;
            movie.LastInfoSync = DateTime.UtcNow;
            movie.Runtime = movieInfo.Runtime;
            movie.Images = movieInfo.Images;
            movie.Ratings = movieInfo.Ratings;
            movie.Collection = movieInfo.Collection;
            movie.Genres = movieInfo.Genres;
            movie.Certification = movieInfo.Certification;
            movie.InCinemas = movieInfo.InCinemas;
            movie.Website = movieInfo.Website;

            movie.Year = movieInfo.Year;
            movie.SecondaryYear = movieInfo.SecondaryYear;
            movie.PhysicalRelease = movieInfo.PhysicalRelease;
            movie.YouTubeTrailerId = movieInfo.YouTubeTrailerId;
            movie.Studio = movieInfo.Studio;
            movie.HasPreDBEntry = movieInfo.HasPreDBEntry;

            try
            {
                movie.Path = new DirectoryInfo(movie.Path).FullName;
                movie.Path = movie.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Couldn't update movie path for " + movie.Path);
            }

            movie.AlternativeTitles = _titleService.UpdateTitles(movieInfo.AlternativeTitles, movie);

            _movieService.UpdateMovie(new List<Movie> { movie }, true);
            _creditService.UpdateCredits(credits, movie);

            _logger.Debug("Finished movie refresh for {0}", movie.Title);
            _eventAggregator.PublishEvent(new MovieUpdatedEvent(movie));
        }

        private void RescanMovie(Movie movie, bool isNew, CommandTrigger trigger)
        {
            var rescanAfterRefresh = _configService.RescanAfterRefresh;
            var shouldRescan = true;

            if (isNew)
            {
                _logger.Trace("Forcing refresh of {0}. Reason: New movie", movie);
                shouldRescan = true;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.Never)
            {
                _logger.Trace("Skipping refresh of {0}. Reason: never rescan after refresh", movie);
                shouldRescan = false;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.AfterManual && trigger != CommandTrigger.Manual)
            {
                _logger.Trace("Skipping refresh of {0}. Reason: not after automatic scans", movie);
                shouldRescan = false;
            }

            if (!shouldRescan)
            {
                return;
            }

            try
            {
                _diskScanService.Scan(movie);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't rescan movie {0}", movie);
            }
        }

        public void Execute(RefreshMovieCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewMovie;
            _eventAggregator.PublishEvent(new MovieRefreshStartingEvent(message.Trigger == CommandTrigger.Manual));

            if (message.MovieId.HasValue)
            {
                var movie = _movieService.GetMovie(message.MovieId.Value);
                var movieData = _movieInfo.GetMovieInfoAsync(movie.TmdbId);

                try
                {
                    RefreshMovieInfo(movie, movieData);
                    RescanMovie(movie, isNew, trigger);
                }
                catch (MovieNotFoundException)
                {
                    _logger.Error("Movie '{0}' (imdbid {1}) was not found, it may have been removed from The Movie Database.", movie.Title, movie.ImdbId);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't refresh info for {0}", movie);
                    RescanMovie(movie, isNew, trigger);
                    throw;
                }
            }
            else
            {
                var allMovie = _movieService.GetAllMovies().OrderBy(c => c.SortTitle).ToList();

                var updatedTMDBMovies = new HashSet<int>();

                if (message.LastStartTime.HasValue && message.LastStartTime.Value.AddDays(14) > DateTime.UtcNow)
                {
                    updatedTMDBMovies = _movieInfo.GetChangedMovies(message.LastStartTime.Value);
                }

                var toRefresh = allMovie
                    .Where(movie => (updatedTMDBMovies.Count == 0 && _checkIfMovieShouldBeRefreshed.ShouldRefresh(movie)) ||
                           updatedTMDBMovies.Contains(movie.TmdbId) ||
                           message.Trigger == CommandTrigger.Manual)
                    .ToDictionary(x => _movieInfo.GetMovieInfoAsync(x.TmdbId), x => x);

                var tasks = toRefresh.Keys.ToList();

                var skipped = allMovie.Except(toRefresh.Values);

                foreach (var movie in skipped)
                {
                    _logger.Info("Skipping refresh of movie: {0}", movie.Title);
                    RescanMovie(movie, false, trigger);
                }

                while (tasks.Count > 0)
                {
                    var finishedTask = Task.WhenAny(tasks).GetAwaiter().GetResult();
                    tasks.Remove(finishedTask);

                    var movie = toRefresh[finishedTask];

                    try
                    {
                        RefreshMovieInfo(movie, finishedTask);
                    }
                    catch (MovieNotFoundException)
                    {
                        _logger.Error("Movie '{0}' (imdbid {1}) was not found, it may have been removed from The Movie Database.", movie.Title, movie.ImdbId);
                        continue;
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't refresh info for {0}", movie);
                    }

                    RescanMovie(movie, false, trigger);
                }
            }
        }
    }
}
