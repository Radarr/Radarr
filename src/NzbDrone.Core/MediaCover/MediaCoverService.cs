using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Configuration.Events;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.MediaCover
{
    public interface IMapCoversToLocal
    {
        void ConvertToLocalUrls(int movieId, IEnumerable<MediaCover> covers, DateTime? added = null);
        string GetCoverPath(int movieId, MediaCoverTypes coverType, int? height = null);
    }

    public class MediaCoverService :
        IHandleAsync<MovieUpdatedEvent>,
        IHandleAsync<MoviesDeletedEvent>,
        IHandleAsync<MovieEditedEvent>,
        IHandleAsync<MoviesBulkEditedEvent>,
        IHandle<ApplicationStartedEvent>,
        IHandleAsync<ConfigSavedEvent>,
        IExecute<ApplyPosterReplacementCommand>,
        IMapCoversToLocal
    {
        private const string PosterReplacementMarker = "poster.replacement";

        private readonly IMediaCoverProxy _mediaCoverProxy;
        private readonly IImageResizer _resizer;
        private readonly IHttpClient _httpClient;
        private readonly IDiskProvider _diskProvider;
        private readonly ICoverExistsSpecification _coverExistsSpecification;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        private readonly IPosterReplacementService _posterReplacementService;
        private readonly IPosterRenderer _posterRenderer;
        private readonly IMovieService _movieService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        private readonly ICached<bool> _coverExistsCache;
        private readonly ICached<string> _posterReplacementCache;
        private readonly string _coverRootFolder;

        private readonly object _posterReplacementLock = new();
        private string _posterReplacementSettings;

        // ImageSharp is slow on ARM (no hardware acceleration on mono yet)
        // So limit the number of concurrent resizing tasks
        private static readonly SemaphoreSlim Semaphore = new((int)Math.Ceiling(Environment.ProcessorCount / 2.0));

        private static readonly TimeSpan CoverExistsCheckWindow = TimeSpan.FromDays(1);

        public MediaCoverService(IMediaCoverProxy mediaCoverProxy,
                                 IImageResizer resizer,
                                 IHttpClient httpClient,
                                 IDiskProvider diskProvider,
                                 IAppFolderInfo appFolderInfo,
                                 ICoverExistsSpecification coverExistsSpecification,
                                 IConfigFileProvider configFileProvider,
                                 IConfigService configService,
                                 IPosterReplacementService posterReplacementService,
                                 IPosterRenderer posterRenderer,
                                 IMovieService movieService,
                                 IManageCommandQueue commandQueueManager,
                                 IEventAggregator eventAggregator,
                                 ICacheManager cacheManager,
                                 Logger logger)
        {
            _mediaCoverProxy = mediaCoverProxy;
            _resizer = resizer;
            _httpClient = httpClient;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _configFileProvider = configFileProvider;
            _configService = configService;
            _posterReplacementService = posterReplacementService;
            _posterRenderer = posterRenderer;
            _movieService = movieService;
            _commandQueueManager = commandQueueManager;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _coverExistsCache = cacheManager.GetCache<bool>(GetType(), "coverExists");
            _posterReplacementCache = cacheManager.GetCache<string>(GetType(), "posterReplacement");
            _coverRootFolder = appFolderInfo.GetMediaCoverPath();
        }

        public string GetCoverPath(int movieId, MediaCoverTypes coverType, int? height = null)
        {
            var heightSuffix = height.HasValue ? $"-{height}" : "";

            return Path.Combine(GetMovieCoverPath(movieId), coverType.ToString().ToLowerInvariant() + heightSuffix + GetExtension(coverType));
        }

        public void ConvertToLocalUrls(int movieId, IEnumerable<MediaCover> covers, DateTime? added = null)
        {
            if (movieId == 0)
            {
                // Movie isn't in Radarr yet, map via a proxy to circumvent referrer issues
                foreach (var mediaCover in covers)
                {
                    mediaCover.Url = _mediaCoverProxy.RegisterUrl(mediaCover.RemoteUrl);
                }
            }
            else
            {
                foreach (var mediaCover in covers)
                {
                    if (mediaCover.CoverType == MediaCoverTypes.Unknown)
                    {
                        continue;
                    }

                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/" + movieId + "/" + mediaCover.CoverType.ToString().ToLowerInvariant() + GetExtension(mediaCover.CoverType);

                    if (mediaCover.RemoteUrl.IsNotNullOrWhiteSpace() && CoverExists(movieId, mediaCover.CoverType, added))
                    {
                        var hashSource = mediaCover.RemoteUrl;

                        if (mediaCover.CoverType == MediaCoverTypes.Poster)
                        {
                            // A replaced poster must not be served from a browser cache holding the original (or vice versa)
                            var replacementSignature = GetCurrentReplacementSignature(movieId);

                            if (replacementSignature.IsNotNullOrWhiteSpace())
                            {
                                hashSource += replacementSignature;
                            }
                        }

                        mediaCover.Url += "?h=" + hashSource.SHA256Hash()[..20];
                    }
                }
            }
        }

        private bool CoverExists(int movieId, MediaCoverTypes coverType, DateTime? added)
        {
            if (!IsRecentlyAdded(added))
            {
                return true;
            }

            var filePath = GetCoverPath(movieId, coverType);

            return _coverExistsCache.Get(filePath, () => _diskProvider.FileExists(filePath));
        }

        private static bool IsRecentlyAdded(DateTime? added)
        {
            return added > DateTime.UtcNow - CoverExistsCheckWindow;
        }

        private void RemoveCoverExistsCache(Movie movie)
        {
            foreach (var cover in movie.MovieMetadata.Value.Images)
            {
                _coverExistsCache.Remove(GetCoverPath(movie.Id, cover.CoverType));
            }
        }

        private string GetMovieCoverPath(int movieId)
        {
            return Path.Combine(_coverRootFolder, movieId.ToString());
        }

        private EnsureCoversResult EnsureCovers(Movie movie, Func<MediaCover, bool> filter = null)
        {
            var result = new EnsureCoversResult();
            var toResize = new List<Tuple<MediaCover, bool>>();

            foreach (var cover in movie.MovieMetadata.Value.Images)
            {
                if (cover.CoverType == MediaCoverTypes.Unknown || (filter != null && !filter(cover)))
                {
                    continue;
                }

                var fileName = GetCoverPath(movie.Id, cover.CoverType);
                var alreadyExists = false;

                if (cover.CoverType == MediaCoverTypes.Poster)
                {
                    var replacement = _posterReplacementService.GetReplacement(movie);

                    try
                    {
                        if (replacement != null)
                        {
                            var rendered = EnsureReplacementPoster(movie, replacement);

                            result.Updated |= rendered;
                            result.PosterReplacementChanged |= rendered;
                            toResize.Add(Tuple.Create(cover, !rendered));

                            continue;
                        }

                        // No longer replaced, remove the generated poster so the original is downloaded again below
                        result.PosterReplacementChanged |= RemoveReplacementPoster(movie);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Couldn't replace poster for {0}", movie);

                        continue;
                    }
                }

                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(cover.RemoteUrl, fileName);

                    if (!alreadyExists)
                    {
                        DownloadCover(movie, cover);
                        result.Updated = true;
                    }

                    if (IsRecentlyAdded(movie.Added))
                    {
                        _coverExistsCache.Set(fileName, true);
                    }
                }
                catch (HttpException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", movie, e.Message);
                }
                catch (WebException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", movie, e.Message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't download media cover for {0}", movie);
                }

                toResize.Add(Tuple.Create(cover, alreadyExists));
            }

            try
            {
                Semaphore.Wait();

                foreach (var tuple in toResize)
                {
                    EnsureResizedCovers(movie, tuple.Item1, !tuple.Item2);
                }
            }
            finally
            {
                Semaphore.Release();
            }

            return result;
        }

        private bool EnsureReplacementPoster(Movie movie, PosterReplacement replacement)
        {
            lock (_posterReplacementLock)
            {
                var posterPath = GetCoverPath(movie.Id, MediaCoverTypes.Poster);
                var signature = replacement.Signature;

                if (GetCurrentReplacementSignature(movie.Id) == signature && _diskProvider.FileExists(posterPath))
                {
                    return false;
                }

                _logger.Info("Replacing poster for {0}", movie);

                _diskProvider.EnsureFolder(GetMovieCoverPath(movie.Id));
                _posterRenderer.Render(replacement, posterPath);
                _diskProvider.WriteAllText(GetPosterReplacementMarkerPath(movie.Id), signature);
                _posterReplacementCache.Set(movie.Id.ToString(), signature);

                if (IsRecentlyAdded(movie.Added))
                {
                    _coverExistsCache.Set(posterPath, true);
                }

                return true;
            }
        }

        private bool RemoveReplacementPoster(Movie movie)
        {
            lock (_posterReplacementLock)
            {
                if (GetCurrentReplacementSignature(movie.Id).IsNullOrWhiteSpace())
                {
                    return false;
                }

                _logger.Info("Restoring original poster for {0}", movie);

                var posterPath = GetCoverPath(movie.Id, MediaCoverTypes.Poster);

                if (_diskProvider.FileExists(posterPath))
                {
                    _diskProvider.DeleteFile(posterPath);
                }

                _diskProvider.DeleteFile(GetPosterReplacementMarkerPath(movie.Id));
                _posterReplacementCache.Set(movie.Id.ToString(), string.Empty);
                _coverExistsCache.Remove(posterPath);

                return true;
            }
        }

        // Signature of the replacement poster currently on disk, empty if the poster is the original
        private string GetCurrentReplacementSignature(int movieId)
        {
            return _posterReplacementCache.Get(movieId.ToString(), () =>
            {
                var markerPath = GetPosterReplacementMarkerPath(movieId);

                return _diskProvider.FileExists(markerPath) ? _diskProvider.ReadAllText(markerPath)?.Trim() ?? string.Empty : string.Empty;
            });
        }

        private bool PosterReplacementOutdated(Movie movie)
        {
            var replacement = _posterReplacementService.GetReplacement(movie);

            return (replacement?.Signature ?? string.Empty) != GetCurrentReplacementSignature(movie.Id);
        }

        private string GetPosterReplacementMarkerPath(int movieId)
        {
            return Path.Combine(GetMovieCoverPath(movieId), PosterReplacementMarker);
        }

        private void ApplyPosterReplacement(Movie movie)
        {
            if (!movie.MovieMetadata.Value.Images.Any(c => c.CoverType == MediaCoverTypes.Poster) || !PosterReplacementOutdated(movie))
            {
                return;
            }

            var result = EnsureCovers(movie, c => c.CoverType == MediaCoverTypes.Poster);

            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(movie, result.Updated || result.PosterReplacementChanged, result.PosterReplacementChanged));
        }

        private string GetPosterReplacementSettings()
        {
            return string.Join("|",
                _configService.PosterReplacementEnabled,
                string.Join(",", _configService.PosterReplacementGenres),
                string.Join(",", _configService.PosterReplacementTags),
                _configService.PosterReplacementBackgroundColor,
                _configService.PosterReplacementTextColor);
        }

        private void DownloadCover(Movie movie, MediaCover cover)
        {
            var fileName = GetCoverPath(movie.Id, cover.CoverType);

            _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, movie, cover.RemoteUrl);
            _httpClient.DownloadFile(cover.RemoteUrl, fileName);
        }

        private void EnsureResizedCovers(Movie movie, MediaCover cover, bool forceResize)
        {
            int[] heights;

            switch (cover.CoverType)
            {
                default:
                    return;

                case MediaCoverTypes.Poster:
                case MediaCoverTypes.Headshot:
                    heights = new[] { 500, 250 };
                    break;

                case MediaCoverTypes.Banner:
                    heights = new[] { 70, 35 };
                    break;

                case MediaCoverTypes.Fanart:
                case MediaCoverTypes.Screenshot:
                    heights = new[] { 360, 180 };
                    break;
            }

            foreach (var height in heights)
            {
                var mainFileName = GetCoverPath(movie.Id, cover.CoverType);
                var resizeFileName = GetCoverPath(movie.Id, cover.CoverType, height);

                if (forceResize || !_diskProvider.FileExists(resizeFileName) || _diskProvider.GetFileSize(resizeFileName) == 0)
                {
                    _logger.Debug("Resizing {0}-{1} for {2}", cover.CoverType, height, movie);

                    try
                    {
                        _resizer.Resize(mainFileName, resizeFileName, height);
                    }
                    catch
                    {
                        _logger.Debug("Couldn't resize media cover {0}-{1} for {2}, using full size image instead.", cover.CoverType, height, movie);
                    }
                }
            }
        }

        private static string GetExtension(MediaCoverTypes coverType)
        {
            return coverType switch
            {
                MediaCoverTypes.Clearlogo => ".png",
                _ => ".jpg"
            };
        }

        public void HandleAsync(MovieUpdatedEvent message)
        {
            var result = EnsureCovers(message.Movie);

            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Movie, result.Updated || result.PosterReplacementChanged, result.PosterReplacementChanged));
        }

        public void HandleAsync(MovieEditedEvent message)
        {
            // Tags may have changed
            ApplyPosterReplacement(message.Movie);
        }

        public void HandleAsync(MoviesBulkEditedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                ApplyPosterReplacement(movie);
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {
            _posterReplacementSettings = GetPosterReplacementSettings();
        }

        public void HandleAsync(ConfigSavedEvent message)
        {
            var settings = GetPosterReplacementSettings();

            if (settings == _posterReplacementSettings)
            {
                return;
            }

            _posterReplacementSettings = settings;
            _commandQueueManager.Push(new ApplyPosterReplacementCommand());
        }

        public void Execute(ApplyPosterReplacementCommand message)
        {
            var movies = _movieService.GetAllMovies().Where(PosterReplacementOutdated).ToList();

            _logger.ProgressInfo("Updating poster replacement for {0} movies", movies.Count);

            foreach (var movie in movies)
            {
                try
                {
                    ApplyPosterReplacement(movie);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't update poster replacement for {0}", movie);
                }
            }

            _logger.ProgressInfo("Updated poster replacement for {0} movies", movies.Count);
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                RemoveCoverExistsCache(movie);
                _posterReplacementCache.Remove(movie.Id.ToString());

                var path = GetMovieCoverPath(movie.Id);
                if (_diskProvider.FolderExists(path))
                {
                    _diskProvider.DeleteFolder(path, true);
                }
            }
        }
    }

    internal class EnsureCoversResult
    {
        public bool Updated { get; set; }
        public bool PosterReplacementChanged { get; set; }
    }
}
