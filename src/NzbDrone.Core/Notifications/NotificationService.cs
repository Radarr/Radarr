using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Update.History.Events;

namespace NzbDrone.Core.Notifications
{
    public class NotificationService
        : IHandle<MovieRenamedEvent>,
          IHandle<MovieGrabbedEvent>,
          IHandle<MovieFileImportedEvent>,
          IHandle<MoviesDeletedEvent>,
          IHandle<MovieAddedEvent>,
          IHandle<MoviesImportedEvent>,
          IHandle<MovieFileDeletedEvent>,
          IHandle<HealthCheckFailedEvent>,
          IHandle<HealthCheckRestoredEvent>,
          IHandle<UpdateInstalledEvent>,
          IHandle<ManualInteractionRequiredEvent>,
          IHandleAsync<DeleteCompletedEvent>,
          IHandleAsync<DownloadsProcessedEvent>,
          IHandleAsync<RenameCompletedEvent>,
          IHandleAsync<HealthCheckCompleteEvent>
    {
        private readonly INotificationFactory _notificationFactory;
        private readonly INotificationStatusService _notificationStatusService;
        private readonly Logger _logger;

        public NotificationService(INotificationFactory notificationFactory, INotificationStatusService notificationStatusService, Logger logger)
        {
            _notificationFactory = notificationFactory;
            _notificationStatusService = notificationStatusService;
            _logger = logger;
        }

        private string GetMessage(Movie movie, QualityModel quality)
        {
            var qualityString = quality.Quality.ToString();
            var imdbUrl = "https://www.imdb.com/title/" + movie.MovieMetadata.Value.ImdbId + "/";

            if (quality.Revision.Version > 1)
            {
                qualityString += " Proper";
            }

            return string.Format("{0} ({1}) [{2}] {3}",
                                    movie.Title,
                                    movie.Year,
                                    qualityString,
                                    imdbUrl);
        }

        private bool ShouldHandleMovie(ProviderDefinition definition, Movie movie)
        {
            if (definition.Tags.Empty())
            {
                _logger.Debug("No tags set for this notification.");
                return true;
            }

            if (definition.Tags.Intersect(movie.Tags).Any())
            {
                _logger.Debug("Notification and movie have one or more intersecting tags.");
                return true;
            }

            // TODO: this message could be more clear
            _logger.Debug("{0} does not have any intersecting tags with {1}. Notification will not be sent", definition.Name, movie.Title);
            return false;
        }

        private bool ShouldHandleHealthFailure(HealthCheck.HealthCheck healthCheck, bool includeWarnings)
        {
            if (healthCheck.Type == HealthCheckResult.Error)
            {
                return true;
            }

            if (healthCheck.Type == HealthCheckResult.Warning && includeWarnings)
            {
                return true;
            }

            return false;
        }

        public void Handle(MovieGrabbedEvent message)
        {
            var grabMessage = new GrabMessage
            {
                Message = GetMessage(message.Movie.Movie, message.Movie.ParsedMovieInfo.Quality),
                Quality = message.Movie.ParsedMovieInfo.Quality,
                Movie = message.Movie.Movie,
                RemoteMovie = message.Movie,
                DownloadClientType = message.DownloadClient,
                DownloadClientName = message.DownloadClientName,
                DownloadId = message.DownloadId
            };

            foreach (var notification in _notificationFactory.OnGrabEnabled())
            {
                try
                {
                    if (!ShouldHandleMovie(notification.Definition, message.Movie.Movie))
                    {
                        continue;
                    }

                    notification.OnGrab(grabMessage);
                    _notificationStatusService.RecordSuccess(notification.Definition.Id);
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Error(ex, "Unable to send OnGrab notification to {0}", notification.Definition.Name);
                }
            }
        }

        public void Handle(MovieFileImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadMessage = new DownloadMessage
            {
                Message = GetMessage(message.MovieInfo.Movie, message.MovieInfo.Quality),
                MovieInfo = message.MovieInfo,
                MovieFile = message.ImportedMovie,
                Movie = message.MovieInfo.Movie,
                OldMovieFiles = message.OldFiles,
                SourcePath = message.MovieInfo.Path,
                DownloadClientInfo = message.DownloadClientInfo,
                DownloadId = message.DownloadId,
                Release = message.MovieInfo.Release
            };

            foreach (var notification in _notificationFactory.OnDownloadEnabled())
            {
                try
                {
                    if (ShouldHandleMovie(notification.Definition, message.MovieInfo.Movie))
                    {
                        if (downloadMessage.OldMovieFiles.Empty() || ((NotificationDefinition)notification.Definition).OnUpgrade)
                        {
                            notification.OnDownload(downloadMessage);
                            _notificationStatusService.RecordSuccess(notification.Definition.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnDownload notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(MovieAddedEvent message)
        {
            foreach (var notification in _notificationFactory.OnMovieAddedEnabled())
            {
                try
                {
                    if (ShouldHandleMovie(notification.Definition, message.Movie))
                    {
                        notification.OnMovieAdded(message.Movie);
                        _notificationStatusService.RecordSuccess(notification.Definition.Id);
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnMovieAdded notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(MoviesImportedEvent message)
        {
            foreach (var notification in _notificationFactory.OnMovieAddedEnabled())
            {
                try
                {
                    foreach (var movie in message.Movies)
                    {
                        if (ShouldHandleMovie(notification.Definition, movie))
                        {
                            notification.OnMovieAdded(movie);
                            _notificationStatusService.RecordSuccess(notification.Definition.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnMovieAdded notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(MovieRenamedEvent message)
        {
            foreach (var notification in _notificationFactory.OnRenameEnabled())
            {
                try
                {
                    if (ShouldHandleMovie(notification.Definition, message.Movie))
                    {
                        notification.OnMovieRename(message.Movie, message.RenamedFiles);
                        _notificationStatusService.RecordSuccess(notification.Definition.Id);
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnRename notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(UpdateInstalledEvent message)
        {
            var updateMessage = new ApplicationUpdateMessage();
            updateMessage.Message = $"Radarr updated from {message.PreviousVerison.ToString()} to {message.NewVersion.ToString()}";
            updateMessage.PreviousVersion = message.PreviousVerison;
            updateMessage.NewVersion = message.NewVersion;

            foreach (var notification in _notificationFactory.OnApplicationUpdateEnabled())
            {
                try
                {
                    notification.OnApplicationUpdate(updateMessage);
                    _notificationStatusService.RecordSuccess(notification.Definition.Id);
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnApplicationUpdate notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(ManualInteractionRequiredEvent message)
        {
            var movie = message.RemoteMovie?.Movie;
            var mess = "";

            if (movie != null)
            {
                mess = GetMessage(movie, message.RemoteMovie.ParsedMovieInfo.Quality);
            }

            if (mess.IsNullOrWhiteSpace() && message.TrackedDownload.DownloadItem != null)
            {
                mess = message.TrackedDownload.DownloadItem.Title;
            }

            if (mess.IsNullOrWhiteSpace())
            {
                return;
            }

            var manualInteractionMessage = new ManualInteractionRequiredMessage
            {
                Message = mess,
                Movie = movie,
                Quality = message.RemoteMovie?.ParsedMovieInfo.Quality,
                RemoteMovie = message.RemoteMovie,
                TrackedDownload = message.TrackedDownload,
                DownloadClientInfo = message.TrackedDownload.DownloadItem?.DownloadClientInfo,
                DownloadId = message.TrackedDownload.DownloadItem?.DownloadId,
                Release = message.Release
            };

            foreach (var notification in _notificationFactory.OnManualInteractionEnabled())
            {
                try
                {
                    if (!ShouldHandleMovie(notification.Definition, message.RemoteMovie.Movie))
                    {
                        continue;
                    }

                    notification.OnManualInteractionRequired(manualInteractionMessage);
                    _notificationStatusService.RecordSuccess(notification.Definition.Id);
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Error(ex, "Unable to send OnManualInteractionRequired notification to {0}", notification.Definition.Name);
                }
            }
        }

        public void Handle(MovieFileDeletedEvent message)
        {
            var deleteMessage = new MovieFileDeleteMessage();
            deleteMessage.Message = GetMessage(message.MovieFile.Movie, message.MovieFile.Quality);
            deleteMessage.MovieFile = message.MovieFile;
            deleteMessage.Movie = message.MovieFile.Movie;
            deleteMessage.Reason = message.Reason;

            foreach (var notification in _notificationFactory.OnMovieFileDeleteEnabled())
            {
                try
                {
                    if (message.Reason != MediaFiles.DeleteMediaFileReason.Upgrade || ((NotificationDefinition)notification.Definition).OnMovieFileDeleteForUpgrade)
                    {
                        if (ShouldHandleMovie(notification.Definition, message.MovieFile.Movie))
                        {
                            notification.OnMovieFileDelete(deleteMessage);
                            _notificationStatusService.RecordSuccess(notification.Definition.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnMovieFileDelete notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(MoviesDeletedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                var deleteMessage = new MovieDeleteMessage(movie, message.DeleteFiles);

                foreach (var notification in _notificationFactory.OnMovieDeleteEnabled())
                {
                    try
                    {
                        if (ShouldHandleMovie(notification.Definition, deleteMessage.Movie))
                        {
                            notification.OnMovieDelete(deleteMessage);
                            _notificationStatusService.RecordSuccess(notification.Definition.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _notificationStatusService.RecordFailure(notification.Definition.Id);
                        _logger.Warn(ex, "Unable to send OnMovieDelete notification to: " + notification.Definition.Name);
                    }
                }
            }
        }

        public void Handle(HealthCheckFailedEvent message)
        {
            // Don't send health check notifications during the start up grace period,
            // once that duration expires they they'll be retested and fired off if necessary.
            if (message.IsInStartupGracePeriod)
            {
                return;
            }

            foreach (var notification in _notificationFactory.OnHealthIssueEnabled())
            {
                try
                {
                    if (ShouldHandleHealthFailure(message.HealthCheck, ((NotificationDefinition)notification.Definition).IncludeHealthWarnings))
                    {
                        notification.OnHealthIssue(message.HealthCheck);
                        _notificationStatusService.RecordSuccess(notification.Definition.Id);
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnHealthIssue notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(HealthCheckRestoredEvent message)
        {
            if (message.IsInStartupGracePeriod)
            {
                return;
            }

            foreach (var notification in _notificationFactory.OnHealthRestoredEnabled())
            {
                try
                {
                    if (ShouldHandleHealthFailure(message.PreviousCheck, ((NotificationDefinition)notification.Definition).IncludeHealthWarnings))
                    {
                        notification.OnHealthRestored(message.PreviousCheck);
                        _notificationStatusService.RecordSuccess(notification.Definition.Id);
                    }
                }
                catch (Exception ex)
                {
                    _notificationStatusService.RecordFailure(notification.Definition.Id);
                    _logger.Warn(ex, "Unable to send OnHealthRestored notification to: " + notification.Definition.Name);
                }
            }
        }

        public void HandleAsync(DeleteCompletedEvent message)
        {
            ProcessQueue();
        }

        public void HandleAsync(DownloadsProcessedEvent message)
        {
            ProcessQueue();
        }

        public void HandleAsync(RenameCompletedEvent message)
        {
            ProcessQueue();
        }

        public void HandleAsync(HealthCheckCompleteEvent message)
        {
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            foreach (var notification in _notificationFactory.GetAvailableProviders())
            {
                try
                {
                    notification.ProcessQueue();
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to process notification queue for " + notification.Definition.Name);
                }
            }
        }
    }
}
