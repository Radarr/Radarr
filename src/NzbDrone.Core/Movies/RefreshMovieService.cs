using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.RadarrAPI;
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
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfMovieShouldBeRefreshed _checkIfMovieShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly IRadarrAPIClient _apiClient;

        private readonly Logger _logger;

        public RefreshMovieService(IProvideMovieInfo movieInfo,
                                    IMovieService movieService,
                                    IAlternativeTitleService titleService,
                                    ICreditService creditService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    IRadarrAPIClient apiClient,
                                    ICheckIfMovieShouldBeRefreshed checkIfMovieShouldBeRefreshed,
                                    IManageCommandQueue commandQueue,
                                    IConfigService configService,
                                    Logger logger)
        {
            _movieInfo = movieInfo;
            _movieService = movieService;
            _titleService = titleService;
            _creditService = creditService;
            _eventAggregator = eventAggregator;
            _apiClient = apiClient;
            _commandQueueManager = commandQueue;
            _diskScanService = diskScanService;
            _checkIfMovieShouldBeRefreshed = checkIfMovieShouldBeRefreshed;
            _configService = configService;
            _logger = logger;
        }

        private void RefreshMovieInfo(Movie movie)
        {
            _logger.ProgressInfo("Updating Info for {0}", movie.Title);

            var tuple = _movieInfo.GetMovieInfo(movie.TmdbId, movie.Profile, movie.HasPreDBEntry);

            var movieInfo = tuple.Item1;

            if (movie.TmdbId != movieInfo.TmdbId)
            {
                _logger.Warn("Movie '{0}' (tvdbid {1}) was replaced with '{2}' (tvdbid {3}), because the original was a duplicate.", movie.Title, movie.TmdbId, movieInfo.Title, movieInfo.TmdbId);
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

            //movie.AlternativeTitles = movieInfo.AlternativeTitles;
            movie.Year = movieInfo.Year;
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

            try
            {
                var mappings = _apiClient.AlternativeTitlesAndYearForMovie(movieInfo.TmdbId);
                var mappingsTitles = mappings.Item1;

                mappingsTitles = mappingsTitles.Where(t => t.IsTrusted()).ToList();

                movieInfo.AlternativeTitles.AddRange(mappingsTitles);

                movie.AlternativeTitles = _titleService.UpdateTitles(movieInfo.AlternativeTitles, movie);

                if (mappings.Item2 != null)
                {
                    movie.SecondaryYear = mappings.Item2.Year;
                    movie.SecondaryYearSourceId = mappings.Item2.SourceId;
                }
                else
                {
                    movie.SecondaryYear = null;
                    movie.SecondaryYearSourceId = 0;
                }
            }
            catch (RadarrAPIException)
            {
                //Not that wild, could just be a 404.
            }
            catch (Exception ex)
            {
                _logger.Info(ex, "Unable to communicate with Mappings Server.");
            }

            _movieService.UpdateMovie(new List<Movie> { movie });
            _creditService.UpdateCredits(tuple.Item2, movie);

            try
            {
                var newTitles = movieInfo.AlternativeTitles.Except(movie.AlternativeTitles);

                //_titleService.AddAltTitles(newTitles.ToList(), movie);
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Failed adding alternative titles.");
                throw;
            }

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

                try
                {
                    RefreshMovieInfo(movie);
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

                foreach (var movie in allMovie)
                {
                    if ((updatedTMDBMovies.Count == 0 && _checkIfMovieShouldBeRefreshed.ShouldRefresh(movie)) || updatedTMDBMovies.Contains(movie.TmdbId) || message.Trigger == CommandTrigger.Manual)
                    {
                        try
                        {
                            RefreshMovieInfo(movie);
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
                    else
                    {
                        _logger.Info("Skipping refresh of movie: {0}", movie.Title);
                        RescanMovie(movie, false, trigger);
                    }
                }
            }
        }
    }
}
