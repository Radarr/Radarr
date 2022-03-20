using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.MediaCover
{
    public interface IMapCoversToLocal
    {
        Dictionary<string, FileInfo> GetCoverFileInfos();
        void ConvertToLocalUrls(int movieId, IEnumerable<MediaCover> covers, Dictionary<string, FileInfo> fileInfos = null);
        void ConvertToLocalUrls(IEnumerable<Tuple<int, IEnumerable<MediaCover>>> items, Dictionary<string, FileInfo> coverFileInfos);
        string GetCoverPath(int movieId, MediaCoverTypes mediaCoverTypes, int? height = null);
    }

    public class MediaCoverService :
        IHandleAsync<MovieUpdatedEvent>,
        IHandleAsync<MoviesDeletedEvent>,
        IMapCoversToLocal
    {
        private readonly IMediaCoverProxy _mediaCoverProxy;
        private readonly IImageResizer _resizer;
        private readonly IHttpClient _httpClient;
        private readonly IDiskProvider _diskProvider;
        private readonly ICoverExistsSpecification _coverExistsSpecification;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        private readonly string _coverRootFolder;

        // ImageSharp is slow on ARM (no hardware acceleration on mono yet)
        // So limit the number of concurrent resizing tasks
        private static SemaphoreSlim _semaphore = new SemaphoreSlim((int)Math.Ceiling(Environment.ProcessorCount / 2.0));

        public MediaCoverService(IMediaCoverProxy mediaCoverProxy,
                                 IImageResizer resizer,
                                 IHttpClient httpClient,
                                 IDiskProvider diskProvider,
                                 IAppFolderInfo appFolderInfo,
                                 ICoverExistsSpecification coverExistsSpecification,
                                 IConfigFileProvider configFileProvider,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _mediaCoverProxy = mediaCoverProxy;
            _resizer = resizer;
            _httpClient = httpClient;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _configFileProvider = configFileProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _coverRootFolder = appFolderInfo.GetMediaCoverPath();
        }

        public string GetCoverPath(int movieId, MediaCoverTypes coverTypes, int? height = null)
        {
            var heightSuffix = height.HasValue ? "-" + height.ToString() : "";

            return Path.Combine(GetMovieCoverPath(movieId), coverTypes.ToString().ToLower() + heightSuffix + ".jpg");
        }

        public Dictionary<string, FileInfo> GetCoverFileInfos()
        {
            if (!_diskProvider.FolderExists(_coverRootFolder))
            {
                return new Dictionary<string, FileInfo>();
            }

            return  _diskProvider
                    .GetFileInfos(_coverRootFolder, SearchOption.AllDirectories)
                    .ToDictionary(x => x.FullName, PathEqualityComparer.Instance);
        }

        public void ConvertToLocalUrls(int movieId, IEnumerable<MediaCover> covers, Dictionary<string, FileInfo> fileInfos = null)
        {
            if (movieId == 0)
            {
                // Movie isn't in Radarr yet, map via a proxy to circument referrer issues
                foreach (var mediaCover in covers)
                {
                    mediaCover.RemoteUrl = mediaCover.Url;
                    mediaCover.Url = _mediaCoverProxy.RegisterUrl(mediaCover.RemoteUrl);
                }
            }
            else
            {
                foreach (var mediaCover in covers)
                {
                    var filePath = GetCoverPath(movieId, mediaCover.CoverType);

                    mediaCover.RemoteUrl = mediaCover.Url;
                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/" + movieId + "/" + mediaCover.CoverType.ToString().ToLower() + ".jpg";

                    FileInfo file;
                    var fileExists = false;
                    if (fileInfos != null)
                    {
                        fileExists = fileInfos.TryGetValue(filePath, out file);
                    }
                    else
                    {
                        file = _diskProvider.GetFileInfo(filePath);
                        fileExists = file.Exists;
                    }

                    if (fileExists)
                    {
                        var lastWrite = file.LastWriteTimeUtc;
                        mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                    }
                }
            }
        }

        public void ConvertToLocalUrls(IEnumerable<Tuple<int, IEnumerable<MediaCover>>> items, Dictionary<string, FileInfo> coverFileInfos)
        {
            foreach (var movie in items)
            {
                ConvertToLocalUrls(movie.Item1, movie.Item2, coverFileInfos);
            }
        }

        private string GetMovieCoverPath(int movieId)
        {
            return Path.Combine(_coverRootFolder, movieId.ToString());
        }

        private bool EnsureCovers(Movie movie)
        {
            bool updated = false;
            var toResize = new List<Tuple<MediaCover, bool>>();

            foreach (var cover in movie.MovieMetadata.Value.Images)
            {
                var fileName = GetCoverPath(movie.Id, cover.CoverType);
                var alreadyExists = false;
                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(cover.Url, fileName);
                    if (!alreadyExists)
                    {
                        DownloadCover(movie, cover);
                        updated = true;
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
                _semaphore.Wait();

                foreach (var tuple in toResize)
                {
                    EnsureResizedCovers(movie, tuple.Item1, !tuple.Item2);
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return updated;
        }

        private void DownloadCover(Movie movie, MediaCover cover)
        {
            var fileName = GetCoverPath(movie.Id, cover.CoverType);

            _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, movie, cover.Url);
            _httpClient.DownloadFile(cover.Url, fileName);
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

        public void HandleAsync(MovieUpdatedEvent message)
        {
            var updated = EnsureCovers(message.Movie);

            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Movie, updated));
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            foreach (var movie in message.Movies)
            {
                var path = GetMovieCoverPath(movie.Id);
                if (_diskProvider.FolderExists(path))
                {
                    _diskProvider.DeleteFolder(path, true);
                }
            }
        }
    }
}
