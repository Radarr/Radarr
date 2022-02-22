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
          IHandle<MovieImportedEvent>,
          IHandle<MoviesDeletedEvent>,
          IHandle<MovieFileDeletedEvent>,
          IHandle<HealthCheckFailedEvent>,
          IHandle<UpdateInstalledEvent>,
          IHandleAsync<DeleteCompletedEvent>,
          IHandleAsync<DownloadsProcessedEvent>,
          IHandleAsync<RenameCompletedEvent>,
          IHandleAsync<HealthCheckCompleteEvent>
    {
        private readonly INotificationFactory _notificationFactory;
        private readonly Logger _logger;

        public NotificationService(INotificationFactory notificationFactory, Logger logger)
        {
            _notificationFactory = notificationFactory;
            _logger = logger;
        }

        private string GetMessage(Movie movie, QualityModel quality)
        {
            var qualityString = quality.Quality.ToString();
            var imdbUrl = "https://www.imdb.com/title/" + movie.ImdbId + "/";

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

            //TODO: this message could be more clear
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
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Unable to send OnGrab notification to {0}", notification.Definition.Name);
                }
            }
        }

        public void Handle(MovieImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadMessage = new DownloadMessage
            {
                Message = GetMessage(message.MovieInfo.Movie, message.MovieInfo.Quality),
                MovieFile = message.ImportedMovie,
                Movie = message.MovieInfo.Movie,
                OldMovieFiles = message.OldFiles,
                SourcePath = message.MovieInfo.Path,
                DownloadClientInfo = message.DownloadClientInfo,
                DownloadId = message.DownloadId
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
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnDownload notification to: " + notification.Definition.Name);
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
                    }
                }
                catch (Exception ex)
                {
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
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnApplicationUpdate notification to: " + notification.Definition.Name);
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
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnMovieFileDelete notification to: " + notification.Definition.Name);
                }
            }
        }

        public void Handle(MoviesDeletedEvent message)
        {
            foreach (Movie movie in message.Movies)
            {
                var deleteMessage = new MovieDeleteMessage(movie, message.DeleteFiles);

                foreach (var notification in _notificationFactory.OnMovieDeleteEnabled())
                {
                    try
                    {
                        if (ShouldHandleMovie(notification.Definition, deleteMessage.Movie))
                        {
                            notification.OnMovieDelete(deleteMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "Unable to send OnMovieDelete notification to: " + notification.Definition.Name);
                    }
                }
            }
        }

        public void Handle(HealthCheckFailedEvent message)
        {
            foreach (var notification in _notificationFactory.OnHealthIssueEnabled())
            {
                try
                {
                    if (ShouldHandleHealthFailure(message.HealthCheck, ((NotificationDefinition)notification.Definition).IncludeHealthWarnings))
                    {
                        notification.OnHealthIssue(message.HealthCheck);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnHealthIssue notification to: " + notification.Definition.Name);
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
