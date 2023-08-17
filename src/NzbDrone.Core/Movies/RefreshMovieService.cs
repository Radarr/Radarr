using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.AutoTagging;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Collections;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Core.Movies.Credits;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Movies
{
    public class RefreshMovieService : IExecute<RefreshMovieCommand>
    {
        private readonly IProvideMovieInfo _movieInfo;
        private readonly IMovieService _movieService;
        private readonly IAddMovieCollectionService _movieCollectionService;
        private readonly IMovieMetadataService _movieMetadataService;
        private readonly IRootFolderService _folderService;
        private readonly IMovieTranslationService _movieTranslationService;
        private readonly IAlternativeTitleService _titleService;
        private readonly ICreditService _creditService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfMovieShouldBeRefreshed _checkIfMovieShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly IAutoTaggingService _autoTaggingService;
        private readonly Logger _logger;

        public RefreshMovieService(IProvideMovieInfo movieInfo,
                                    IMovieService movieService,
                                    IAddMovieCollectionService movieCollectionService,
                                    IMovieMetadataService movieMetadataService,
                                    IRootFolderService folderService,
                                    IMovieTranslationService movieTranslationService,
                                    IAlternativeTitleService titleService,
                                    ICreditService creditService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    ICheckIfMovieShouldBeRefreshed checkIfMovieShouldBeRefreshed,
                                    IConfigService configService,
                                    IAutoTaggingService autoTaggingService,
                                    Logger logger)
        {
            _movieInfo = movieInfo;
            _movieService = movieService;
            _movieCollectionService = movieCollectionService;
            _movieMetadataService = movieMetadataService;
            _folderService = folderService;
            _movieTranslationService = movieTranslationService;
            _titleService = titleService;
            _creditService = creditService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfMovieShouldBeRefreshed = checkIfMovieShouldBeRefreshed;
            _configService = configService;
            _autoTaggingService = autoTaggingService;
            _logger = logger;
        }

        private Movie RefreshMovieInfo(int movieId)
        {
            // Get the movie before updating, that way any changes made to the movie after the refresh started,
            // but before this movie was refreshed won't be lost.
            var movie = _movieService.GetMovie(movieId);
            var movieMetadata = _movieMetadataService.Get(movie.MovieMetadataId);

            _logger.ProgressInfo("Updating info for {0}", movie.Title);

            MovieMetadata movieInfo;
            List<Credit> credits;

            try
            {
                var tuple = _movieInfo.GetMovieInfo(movie.TmdbId);
                movieInfo = tuple.Item1;
                credits = tuple.Item2;
            }
            catch (MovieNotFoundException)
            {
                if (movieMetadata.Status != MovieStatusType.Deleted)
                {
                    movieMetadata.Status = MovieStatusType.Deleted;
                    _movieMetadataService.Upsert(movieMetadata);
                    _logger.Debug("Movie marked as deleted on TMDb for {0}", movie.Title);
                    _eventAggregator.PublishEvent(new MovieUpdatedEvent(movie));
                }

                throw;
            }

            if (movieMetadata.TmdbId != movieInfo.TmdbId)
            {
                _logger.Warn("Movie '{0}' (TMDb: {1}) was replaced with '{2}' (TMDb: {3}), because the original was a duplicate.", movie.Title, movie.TmdbId, movieInfo.Title, movieInfo.TmdbId);
                movieMetadata.TmdbId = movieInfo.TmdbId;
            }

            movieMetadata.Title = movieInfo.Title;
            movieMetadata.ImdbId = movieInfo.ImdbId;
            movieMetadata.Overview = movieInfo.Overview;
            movieMetadata.Status = movieInfo.Status;
            movieMetadata.Images = movieInfo.Images;
            movieMetadata.CleanTitle = movieInfo.CleanTitle;
            movieMetadata.SortTitle = movieInfo.SortTitle;
            movieMetadata.LastInfoSync = DateTime.UtcNow;
            movieMetadata.Runtime = movieInfo.Runtime;
            movieMetadata.Ratings = movieInfo.Ratings;

            // movie.Genres = movieInfo.Genres;
            movieMetadata.Certification = movieInfo.Certification;
            movieMetadata.InCinemas = movieInfo.InCinemas;
            movieMetadata.Website = movieInfo.Website;

            movieMetadata.Year = movieInfo.Year;
            movieMetadata.SecondaryYear = movieInfo.SecondaryYear;
            movieMetadata.PhysicalRelease = movieInfo.PhysicalRelease;
            movieMetadata.DigitalRelease = movieInfo.DigitalRelease;
            movieMetadata.YouTubeTrailerId = movieInfo.YouTubeTrailerId;
            movieMetadata.Studio = movieInfo.Studio;
            movieMetadata.OriginalTitle = movieInfo.OriginalTitle;
            movieMetadata.CleanOriginalTitle = movieInfo.CleanOriginalTitle;
            movieMetadata.OriginalLanguage = movieInfo.OriginalLanguage;
            movieMetadata.Recommendations = movieInfo.Recommendations;
            movieMetadata.Popularity = movieInfo.Popularity;

            // add collection
            if (movieInfo.CollectionTmdbId > 0)
            {
                var newCollection = _movieCollectionService.AddMovieCollection(new MovieCollection
                {
                    TmdbId = movieInfo.CollectionTmdbId,
                    Title = movieInfo.CollectionTitle,
                    Monitored = movie.AddOptions?.Monitor == MonitorTypes.MovieAndCollection,
                    SearchOnAdd = movie.AddOptions?.SearchForMovie ?? false,
                    QualityProfileId = movie.QualityProfileId,
                    MinimumAvailability = movie.MinimumAvailability,
                    RootFolderPath = _folderService.GetBestRootFolderPath(movie.Path).TrimEnd('/', '\\', ' '),
                    Tags = movie.Tags
                });

                if (newCollection != null)
                {
                    movieMetadata.CollectionTmdbId = newCollection.TmdbId;
                    movieMetadata.CollectionTitle = newCollection.Title;
                }
            }

            movieMetadata.AlternativeTitles = _titleService.UpdateTitles(movieInfo.AlternativeTitles, movieMetadata);
            _movieTranslationService.UpdateTranslations(movieInfo.Translations, movieMetadata);

            _movieMetadataService.Upsert(movieMetadata);
            _creditService.UpdateCredits(credits, movieMetadata);

            movie.MovieMetadata = movieMetadata;

            _logger.Debug("Finished movie metadata refresh for {0}", movieMetadata.Title);
            _eventAggregator.PublishEvent(new MovieUpdatedEvent(movie));

            return movie;
        }

        private void RescanMovie(Movie movie, bool isNew, CommandTrigger trigger)
        {
            var rescanAfterRefresh = _configService.RescanAfterRefresh;
            var shouldRescan = true;

            if (isNew)
            {
                _logger.Trace("Forcing rescan of {0}. Reason: New movie", movie);
                shouldRescan = true;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.Never)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: Never rescan after refresh", movie);
                shouldRescan = false;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.AfterManual && trigger != CommandTrigger.Manual)
            {
                _logger.Trace("Skipping rescan of {0}. Reason: Not after automatic scans", movie);
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

        private void UpdateTags(Movie movie)
        {
            _logger.Trace("Updating tags for {0}", movie);

            var tagsAdded = new HashSet<int>();
            var tagsRemoved = new HashSet<int>();
            var changes = _autoTaggingService.GetTagChanges(movie);

            foreach (var tag in changes.TagsToRemove)
            {
                if (movie.Tags.Contains(tag))
                {
                    movie.Tags.Remove(tag);
                    tagsRemoved.Add(tag);
                }
            }

            foreach (var tag in changes.TagsToAdd)
            {
                if (!movie.Tags.Contains(tag))
                {
                    movie.Tags.Add(tag);
                    tagsAdded.Add(tag);
                }
            }

            if (tagsAdded.Any() || tagsRemoved.Any())
            {
                _movieService.UpdateMovie(movie);
                _logger.Debug("Updated tags for '{0}'. Added: {1}, Removed: {2}", movie.Title, tagsAdded.Count, tagsRemoved.Count);
            }
        }

        public void Execute(RefreshMovieCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewMovie;
            _eventAggregator.PublishEvent(new MovieRefreshStartingEvent(message.Trigger == CommandTrigger.Manual));

            if (message.MovieIds.Any())
            {
                foreach (var movieId in message.MovieIds)
                {
                    var movie = _movieService.GetMovie(movieId);

                    try
                    {
                        movie = RefreshMovieInfo(movieId);
                        UpdateTags(movie);
                        RescanMovie(movie, isNew, trigger);
                    }
                    catch (MovieNotFoundException)
                    {
                        _logger.Error("Movie '{0}' (TMDb {1}) was not found, it may have been removed from The Movie Database.", movie.Title, movie.TmdbId);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't refresh info for {0}", movie);
                        UpdateTags(movie);
                        RescanMovie(movie, isNew, trigger);
                        throw;
                    }
                }
            }
            else
            {
                // TODO refresh all moviemetadata here, even if not used by a Movie
                var allMovie = _movieService.GetAllMovies().OrderBy(c => c.MovieMetadata.Value.SortTitle).ToList();

                var updatedTMDBMovies = new HashSet<int>();

                if (message.LastStartTime.HasValue && message.LastStartTime.Value.AddDays(14) > DateTime.UtcNow)
                {
                    updatedTMDBMovies = _movieInfo.GetChangedMovies(message.LastStartTime.Value);
                }

                foreach (var movie in allMovie)
                {
                    var movieLocal = movie;
                    if ((updatedTMDBMovies.Count == 0 && _checkIfMovieShouldBeRefreshed.ShouldRefresh(movie.MovieMetadata)) || updatedTMDBMovies.Contains(movie.TmdbId) || message.Trigger == CommandTrigger.Manual)
                    {
                        try
                        {
                            movieLocal = RefreshMovieInfo(movieLocal.Id);
                        }
                        catch (MovieNotFoundException)
                        {
                            _logger.Error("Movie '{0}' (TMDb {1}) was not found, it may have been removed from The Movie Database.", movieLocal.Title, movieLocal.TmdbId);
                            continue;
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", movieLocal);
                        }

                        UpdateTags(movie);
                        RescanMovie(movieLocal, false, trigger);
                    }
                    else
                    {
                        _logger.Debug("Skipping refresh of movie: {0}", movieLocal.Title);
                        UpdateTags(movie);
                        RescanMovie(movieLocal, false, trigger);
                    }
                }
            }

            _eventAggregator.PublishEvent(new MovieRefreshCompleteEvent());
        }
    }
}
