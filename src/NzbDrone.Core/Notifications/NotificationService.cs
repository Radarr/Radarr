using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.ThingiProvider;

namespace NzbDrone.Core.Notifications
{
    public class NotificationService
        : IHandle<MovieRenamedEvent>,
          IHandle<MovieGrabbedEvent>,
          IHandle<MovieDownloadedEvent>,
          IHandle<HealthCheckFailedEvent>
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
                DownloadClient = message.DownloadClient,
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

        public void Handle(MovieDownloadedEvent message)
        {
            var downloadMessage = new DownloadMessage();
            downloadMessage.Message = GetMessage(message.Movie.Movie, message.Movie.Quality);
            downloadMessage.MovieFile = message.MovieFile;
            downloadMessage.Movie = message.Movie.Movie;
            downloadMessage.OldMovieFiles = message.OldFiles;
            downloadMessage.SourcePath = message.Movie.Path;
            downloadMessage.DownloadId = message.DownloadId;

            foreach (var notification in _notificationFactory.OnDownloadEnabled())
            {
                try
                {
                    if (ShouldHandleMovie(notification.Definition, message.Movie.Movie))
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
                        notification.OnMovieRename(message.Movie);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(ex, "Unable to send OnRename notification to: " + notification.Definition.Name);
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
    }
}
