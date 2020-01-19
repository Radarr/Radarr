using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.MediaCover
{
    public interface IMapCoversToLocal
    {
        void ConvertToLocalUrls(int movieId, IEnumerable<MediaCover> covers);
        string GetCoverPath(int movieId, MediaCoverTypes mediaCoverTypes, int? height = null);
    }

    public class MediaCoverService :
        IHandleAsync<MovieUpdatedEvent>,
        //IHandleAsync<MovieAddedEvent>,
        IHandleAsync<MovieDeletedEvent>,
        IExecute<EnsureMediaCoversCommand>,
        IMapCoversToLocal
    {
        private readonly IImageResizer _resizer;
        private readonly IHttpClient _httpClient;
        private readonly IDiskProvider _diskProvider;
        private readonly ICoverExistsSpecification _coverExistsSpecification;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueue;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        private readonly string _coverRootFolder;

        public MediaCoverService(IImageResizer resizer,
                                 IHttpClient httpClient,
                                 IDiskProvider diskProvider,
                                 IAppFolderInfo appFolderInfo,
                                 ICoverExistsSpecification coverExistsSpecification,
                                 IConfigFileProvider configFileProvider,
                                 IEventAggregator eventAggregator,
                                 IManageCommandQueue commandQueue,
                                 IMovieService movieService,
                                 Logger logger)
        {
            _resizer = resizer;
            _httpClient = httpClient;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _configFileProvider = configFileProvider;
            _eventAggregator = eventAggregator;
            _commandQueue = commandQueue;
            _movieService = movieService;
            _logger = logger;

            _coverRootFolder = appFolderInfo.GetMediaCoverPath();
        }

        public string GetCoverPath(int movieId, MediaCoverTypes coverTypes, int? height = null)
        {
            var heightSuffix = height.HasValue ? "-" + height.ToString() : "";

            return Path.Combine(GetMovieCoverPath(movieId), coverTypes.ToString().ToLower() + heightSuffix + ".jpg");
        }

        public void ConvertToLocalUrls(int movieId, IEnumerable<MediaCover> covers)
        {
            foreach (var mediaCover in covers)
            {
                var filePath = GetCoverPath(movieId, mediaCover.CoverType);

                mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/" + movieId + "/" + mediaCover.CoverType.ToString().ToLower() + ".jpg";

                /*if (_diskProvider.FileExists(filePath))
                {
                    var lastWrite = _diskProvider.FileGetLastWrite(filePath);
                    mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                }*/
            }
        }

        private string GetMovieCoverPath(int movieId)
        {
            return Path.Combine(_coverRootFolder, movieId.ToString());
        }

        private void EnsureCovers(Movie movie, int retried = 0)
        {
            foreach (var cover in movie.Images)
            {
                var fileName = GetCoverPath(movie.Id, cover.CoverType);
                var alreadyExists = false;
                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(cover.Url, fileName);
                    if (!alreadyExists)
                    {
                        DownloadCover(movie, cover);
                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        _logger.Warn(e, string.Format("Couldn't download media cover for {0}, likely the cover doesn't exist for this movie. {1}", movie, e.Message));
                    }
                    else
                    {
                        _logger.Warn(e, string.Format("Couldn't download media cover for {0}. {1}", movie, e.Message));
                        if (retried < 3)
                        {
                            retried += 1; 
                            _logger.Warn("Retrying for the {0}. time in ten seconds.", retried);
                            System.Threading.Thread.Sleep(10 * 1000);
                            EnsureCovers(movie, retried);
                        }
                        else
                        {
                            _logger.Warn(e, "Couldn't download media cover even after retrying five times :(.");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't download media cover for {0}", movie);
                }

                EnsureResizedCovers(movie, cover, !alreadyExists);
            }
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

        public void Execute(EnsureMediaCoversCommand command)
        {
            var movie = _movieService.GetMovie(command.MovieId);
            EnsureCovers(movie);
            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(movie));
        }

        public void HandleAsync(MovieUpdatedEvent message)
        {
            //EnsureCovers(message.Movie);
            _logger.Info("Testing: {0}, {1}", _commandQueue, message.Movie.Id);
            _commandQueue.Push(new EnsureMediaCoversCommand(message.Movie.Id));
            //_eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Movie));
        }

        public void HandleAsync(MovieAddedEvent message)
        {
            //EnsureCovers(message.Movie);
            //_commandQueue.Push(new EnsureMediaCoversCommand(message.Movie.Id));
            //_eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Movie));
        }

        public void HandleAsync(MovieDeletedEvent message)
        {
            var path = GetMovieCoverPath(message.Movie.Id);
            if (_diskProvider.FolderExists(path))
            {
                _diskProvider.DeleteFolder(path, true);
            }
        }
    }
}
