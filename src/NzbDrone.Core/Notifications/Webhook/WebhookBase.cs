using System.Collections.Generic;
using System.IO;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Localization;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Tags;

namespace NzbDrone.Core.Notifications.Webhook
{
    public abstract class WebhookBase<TSettings> : NotificationBase<TSettings>
        where TSettings : NotificationSettingsBase<TSettings>, new()
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        protected readonly ILocalizationService _localizationService;
        private readonly ITagRepository _tagRepository;
        private readonly IMapCoversToLocal _mediaCoverService;

        protected WebhookBase(IConfigFileProvider configFileProvider, IConfigService configService, ILocalizationService localizationService, ITagRepository tagRepository, IMapCoversToLocal mediaCoverService)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _localizationService = localizationService;
            _tagRepository = tagRepository;
            _mediaCoverService = mediaCoverService;
        }

        protected WebhookGrabPayload BuildOnGrabPayload(GrabMessage message)
        {
            var remoteMovie = message.RemoteMovie;
            var quality = message.Quality;

            return new WebhookGrabPayload
            {
                EventType = WebhookEventType.Grab,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Movie = GetMovie(message.Movie),
                RemoteMovie = new WebhookRemoteMovie(remoteMovie),
                Release = new WebhookRelease(quality, remoteMovie),
                DownloadClient = message.DownloadClientName,
                DownloadClientType = message.DownloadClientType,
                DownloadId = message.DownloadId,
                CustomFormatInfo = new WebhookCustomFormatInfo(remoteMovie.CustomFormats, remoteMovie.CustomFormatScore)
            };
        }

        protected WebhookImportPayload BuildOnDownloadPayload(DownloadMessage message)
        {
            var movieFile = message.MovieFile;

            var payload = new WebhookImportPayload
            {
                EventType = WebhookEventType.Download,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Movie = GetMovie(message.Movie),
                RemoteMovie = new WebhookRemoteMovie(message.Movie),
                MovieFile = new WebhookMovieFile(movieFile),
                Release = new WebhookGrabbedRelease(message.Release),
                IsUpgrade = message.OldMovieFiles.Any(),
                DownloadClient = message.DownloadClientInfo?.Name,
                DownloadClientType = message.DownloadClientInfo?.Type,
                DownloadId = message.DownloadId,
                CustomFormatInfo = new WebhookCustomFormatInfo(message.MovieInfo.CustomFormats, message.MovieInfo.CustomFormatScore)
            };

            if (message.OldMovieFiles.Any())
            {
                payload.DeletedFiles = message.OldMovieFiles.ConvertAll(x =>
                    new WebhookMovieFile(x.MovieFile)
                    {
                        Path = Path.Combine(message.Movie.Path, x.MovieFile.RelativePath),
                        RecycleBinPath = x.RecycleBinPath
                    });
            }

            return payload;
        }

        protected WebhookAddedPayload BuildOnMovieAdded(Movie movie)
        {
            return new WebhookAddedPayload
            {
                EventType = WebhookEventType.MovieAdded,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Movie = GetMovie(movie),
                AddMethod = movie.AddOptions.AddMethod
            };
        }

        protected WebhookMovieFileDeletePayload BuildOnMovieFileDelete(MovieFileDeleteMessage deleteMessage)
        {
            return new WebhookMovieFileDeletePayload
            {
                EventType = WebhookEventType.MovieFileDelete,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Movie = GetMovie(deleteMessage.Movie),
                MovieFile = new WebhookMovieFile(deleteMessage.MovieFile),
                DeleteReason = deleteMessage.Reason
            };
        }

        protected WebhookMovieDeletePayload BuildOnMovieDelete(MovieDeleteMessage deleteMessage)
        {
            var payload = new WebhookMovieDeletePayload
            {
                EventType = WebhookEventType.MovieDelete,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Movie = GetMovie(deleteMessage.Movie),
                DeletedFiles = deleteMessage.DeletedFiles
            };

            if (deleteMessage.DeletedFiles && deleteMessage.Movie.MovieFile != null)
            {
                payload.MovieFolderSize = deleteMessage.Movie.MovieFile.Size;
            }

            return payload;
        }

        protected WebhookRenamePayload BuildOnRenamePayload(Movie movie, List<RenamedMovieFile> renamedFiles)
        {
            return new WebhookRenamePayload
            {
                EventType = WebhookEventType.Rename,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Movie = GetMovie(movie),
                RenamedMovieFiles = renamedFiles.ConvertAll(x => new WebhookRenamedMovieFile(x))
            };
        }

        protected WebhookHealthPayload BuildHealthPayload(HealthCheck.HealthCheck healthCheck)
        {
            return new WebhookHealthPayload
            {
                EventType = WebhookEventType.Health,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Level = healthCheck.Type,
                Message = healthCheck.Message,
                Type = healthCheck.Source.Name,
                WikiUrl = healthCheck.WikiUrl?.ToString()
            };
        }

        protected WebhookHealthPayload BuildHealthRestoredPayload(HealthCheck.HealthCheck healthCheck)
        {
            return new WebhookHealthPayload
            {
                EventType = WebhookEventType.HealthRestored,
                InstanceName = _configFileProvider.InstanceName,
                Level = healthCheck.Type,
                Message = healthCheck.Message,
                Type = healthCheck.Source.Name,
                WikiUrl = healthCheck.WikiUrl?.ToString()
            };
        }

        protected WebhookApplicationUpdatePayload BuildApplicationUpdatePayload(ApplicationUpdateMessage updateMessage)
        {
            return new WebhookApplicationUpdatePayload
            {
                EventType = WebhookEventType.ApplicationUpdate,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Message = updateMessage.Message,
                PreviousVersion = updateMessage.PreviousVersion.ToString(),
                NewVersion = updateMessage.NewVersion.ToString()
            };
        }

        protected WebhookManualInteractionPayload BuildManualInteractionRequiredPayload(ManualInteractionRequiredMessage message)
        {
            var remoteMovie = message.RemoteMovie;
            var quality = message.Quality;

            return new WebhookManualInteractionPayload
            {
                EventType = WebhookEventType.ManualInteractionRequired,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Movie = GetMovie(message.Movie),
                DownloadInfo = new WebhookDownloadClientItem(quality, message.TrackedDownload.DownloadItem),
                DownloadClient = message.DownloadClientInfo?.Name,
                DownloadClientType = message.DownloadClientInfo?.Type,
                DownloadId = message.DownloadId,
                DownloadStatus = message.TrackedDownload.Status.ToString(),
                DownloadStatusMessages = message.TrackedDownload.StatusMessages.Select(x => new WebhookDownloadStatusMessage(x)).ToList(),
                CustomFormatInfo = new WebhookCustomFormatInfo(remoteMovie.CustomFormats, remoteMovie.CustomFormatScore),
                Release = new WebhookGrabbedRelease(message.Release)
            };
        }

        protected WebhookPayload BuildTestPayload()
        {
            return new WebhookGrabPayload
            {
                EventType = WebhookEventType.Test,
                InstanceName = _configFileProvider.InstanceName,
                ApplicationUrl = _configService.ApplicationUrl,
                Movie = new WebhookMovie
                {
                    Id = 1,
                    Title = "Test Title",
                    Year = 1970,
                    FolderPath = "C:\\testpath",
                    ReleaseDate = "1970-01-01",
                    Tags = new List<string> { "test-tag" }
                },
                RemoteMovie = new WebhookRemoteMovie
                {
                    TmdbId = 1234,
                    ImdbId = "5678",
                    Title = "Test title",
                    Year = 1970
                },
                Release = new WebhookRelease
                {
                    Indexer = "Test Indexer",
                    Quality = "Test Quality",
                    QualityVersion = 1,
                    ReleaseGroup = "Test Group",
                    ReleaseTitle = "Test Title",
                    Size = 9999999
                }
            };
        }

        private WebhookMovie GetMovie(Movie movie)
        {
            if (movie == null)
            {
                return null;
            }

            _mediaCoverService.ConvertToLocalUrls(movie.Id, movie.MovieMetadata.Value.Images);

            return new WebhookMovie(movie, GetTagLabels(movie));
        }

        private List<string> GetTagLabels(Movie movie)
        {
            if (movie == null)
            {
                return null;
            }

            return _tagRepository.GetTags(movie.Tags)
                .Select(t => t.Label)
                .Where(l => l.IsNotNullOrWhiteSpace())
                .OrderBy(l => l)
                .ToList();
        }
    }
}
