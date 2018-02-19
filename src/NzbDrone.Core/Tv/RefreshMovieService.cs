using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
//using NzbDrone.Core.DataAugmentation.DailyMovie;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MetadataSource.RadarrAPI;
using NzbDrone.Core.Movies.AlternativeTitles;

namespace NzbDrone.Core.Tv
{
    public class RefreshMovieService : IExecute<RefreshMovieCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IMovieService _movieService;
        private readonly IAlternativeTitleService _titleService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfMovieShouldBeRefreshed _checkIfMovieShouldBeRefreshed;
        private readonly IRadarrAPIClient _apiClient;

        private readonly Logger _logger;

        public RefreshMovieService(IProvideMovieInfo movieInfo,
                                    IMovieService movieService,
                                    IAlternativeTitleService titleService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    IRadarrAPIClient apiClient,
                                    ICheckIfMovieShouldBeRefreshed checkIfMovieShouldBeRefreshed,
                                    IManageCommandQueue commandQueue,
                                    Logger logger)
        {
            _movieInfo = movieInfo;
            _movieService = movieService;
            _titleService = titleService;
            _eventAggregator = eventAggregator;
            _apiClient = apiClient;
            _commandQueueManager = commandQueue;
            _diskScanService = diskScanService;
            _checkIfMovieShouldBeRefreshed = checkIfMovieShouldBeRefreshed;
            _logger = logger;
        }

        private void RefreshMovieInfo(Movie movie)
        {
            _logger.ProgressInfo("Updating Info for {0}", movie.Title);

            Movie movieInfo;

            try
            {
                movieInfo = _movieInfo.GetMovieInfo(movie.TmdbId, movie.Profile, movie.HasPreDBEntry);
            }
            catch (MovieNotFoundException)
            {
                _logger.Error("Movie '{0}' (imdbid {1}) was not found, it may have been removed from TheTVDB.", movie.Title, movie.ImdbId);
                return;
            }

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
            movie.Actors = movieInfo.Actors;
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

            movieInfo.AlternativeTitles = movieInfo.AlternativeTitles.Where(t => t.CleanTitle != movie.CleanTitle)
                .DistinctBy(t => t.CleanTitle)
                .ExceptBy(t => t.CleanTitle, movie.AlternativeTitles, t => t.CleanTitle, EqualityComparer<string>.Default).ToList();

            try
            {
                movie.AlternativeTitles.AddRange(_titleService.AddAltTitles(movieInfo.AlternativeTitles, movie));
                
                var mappings = _apiClient.AlternativeTitlesAndYearForMovie(movieInfo.TmdbId);
                var mappingsTitles = mappings.Item1;

                _titleService.DeleteNotEnoughVotes(mappingsTitles);

                mappingsTitles = mappingsTitles.ExceptBy(t => t.CleanTitle, movie.AlternativeTitles,
                    t => t.CleanTitle, EqualityComparer<string>.Default).ToList();


                mappingsTitles = mappingsTitles.Where(t => t.Votes > 3).ToList();

                movie.AlternativeTitles.AddRange(_titleService.AddAltTitles(mappingsTitles, movie));

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
            catch (RadarrAPIException ex)
            {
                //Not that wild, could just be a 404.
            }
            catch (Exception ex)
            {
                _logger.Info(ex, "Unable to communicate with Mappings Server.");
            }


            _movieService.UpdateMovie(movie);

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

        public void Execute(RefreshMovieCommand message)
        {
            _eventAggregator.PublishEvent(new MovieRefreshStartingEvent(message.Trigger == CommandTrigger.Manual));

            if (message.MovieId.HasValue)
            {
                var movie = _movieService.GetMovie(message.MovieId.Value);
                RefreshMovieInfo(movie);
            }
            else
            {
                var allMovie = _movieService.GetAllMovies().OrderBy(c => c.SortTitle).ToList();

                foreach (var movie in allMovie)
                {
                    if (message.Trigger == CommandTrigger.Manual || _checkIfMovieShouldBeRefreshed.ShouldRefresh(movie))
                    {
                        try
                        {
                            RefreshMovieInfo(movie);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}".Inject(movie));
                        }
                    }

                    else
                    {
                        try
                        {
                            _logger.Info("Skipping refresh of movie: {0}", movie.Title);
                            _commandQueueManager.Push(new RenameMovieFolderCommand(new List<int> { movie.Id }));
                            _diskScanService.Scan(movie);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't rescan movie {0}".Inject(movie));
                        }
                    }
                }
            }
        }
    }
}
