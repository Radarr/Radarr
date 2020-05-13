using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.MediaCover
{
    public interface IMapCoversToLocal
    {
        void ConvertToLocalUrls(int entityId, MediaCoverEntity coverEntity, IEnumerable<MediaCover> covers);
        string GetCoverPath(int entityId, MediaCoverEntity coverEntity, MediaCoverTypes mediaCoverTypes, string extension, int? height = null);
        void EnsureAlbumCovers(Book book);
    }

    public class MediaCoverService :
        IHandleAsync<AuthorRefreshCompleteEvent>,
        IHandleAsync<AuthorDeletedEvent>,
        IHandleAsync<BookDeletedEvent>,
        IMapCoversToLocal
    {
        private const double HTTP_RATE_LIMIT = 0;
        private const string USER_AGENT = "Dalvik/2.1.0 (Linux; U; Android 10; Mi A2 Build/QKQ1.190910.002)";

        private readonly IImageResizer _resizer;
        private readonly IBookService _bookService;
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

        public MediaCoverService(IImageResizer resizer,
                                 IBookService bookService,
                                 IHttpClient httpClient,
                                 IDiskProvider diskProvider,
                                 IAppFolderInfo appFolderInfo,
                                 ICoverExistsSpecification coverExistsSpecification,
                                 IConfigFileProvider configFileProvider,
                                 IEventAggregator eventAggregator,
                                 Logger logger)
        {
            _resizer = resizer;
            _bookService = bookService;
            _httpClient = httpClient;
            _diskProvider = diskProvider;
            _coverExistsSpecification = coverExistsSpecification;
            _configFileProvider = configFileProvider;
            _eventAggregator = eventAggregator;
            _logger = logger;

            _coverRootFolder = appFolderInfo.GetMediaCoverPath();
        }

        public string GetCoverPath(int entityId, MediaCoverEntity coverEntity, MediaCoverTypes coverTypes, string extension, int? height = null)
        {
            var heightSuffix = height.HasValue ? "-" + height.ToString() : "";

            if (coverEntity == MediaCoverEntity.Book)
            {
                return Path.Combine(GetAlbumCoverPath(entityId), coverTypes.ToString().ToLower() + heightSuffix + extension);
            }
            else
            {
                return Path.Combine(GetArtistCoverPath(entityId), coverTypes.ToString().ToLower() + heightSuffix + extension);
            }
        }

        public void ConvertToLocalUrls(int entityId, MediaCoverEntity coverEntity, IEnumerable<MediaCover> covers)
        {
            foreach (var mediaCover in covers)
            {
                var filePath = GetCoverPath(entityId, coverEntity, mediaCover.CoverType, mediaCover.Extension, null);

                if (coverEntity == MediaCoverEntity.Book)
                {
                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/Albums/" + entityId + "/" + mediaCover.CoverType.ToString().ToLower() + mediaCover.Extension;
                }
                else
                {
                    mediaCover.Url = _configFileProvider.UrlBase + @"/MediaCover/" + entityId + "/" + mediaCover.CoverType.ToString().ToLower() + mediaCover.Extension;
                }

                if (_diskProvider.FileExists(filePath))
                {
                    var lastWrite = _diskProvider.FileGetLastWrite(filePath);
                    mediaCover.Url += "?lastWrite=" + lastWrite.Ticks;
                }
            }
        }

        private string GetArtistCoverPath(int authorId)
        {
            return Path.Combine(_coverRootFolder, authorId.ToString());
        }

        private string GetAlbumCoverPath(int bookId)
        {
            return Path.Combine(_coverRootFolder, "Albums", bookId.ToString());
        }

        private void EnsureArtistCovers(Author author)
        {
            var toResize = new List<Tuple<MediaCover, bool>>();

            foreach (var cover in author.Metadata.Value.Images)
            {
                var fileName = GetCoverPath(author.Id, MediaCoverEntity.Author, cover.CoverType, cover.Extension);
                var alreadyExists = false;

                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(null, null, fileName);

                    if (!alreadyExists)
                    {
                        DownloadCover(author, cover, DateTime.Now);
                    }
                }
                catch (WebException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", author, e.Message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't download media cover for {0}", author);
                }

                toResize.Add(Tuple.Create(cover, alreadyExists));
            }

            try
            {
                _semaphore.Wait();

                foreach (var tuple in toResize)
                {
                    EnsureResizedCovers(author, tuple.Item1, !tuple.Item2);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void EnsureAlbumCovers(Book book)
        {
            foreach (var cover in book.Images.Where(e => e.CoverType == MediaCoverTypes.Cover))
            {
                var fileName = GetCoverPath(book.Id, MediaCoverEntity.Book, cover.CoverType, cover.Extension, null);
                var alreadyExists = false;
                try
                {
                    alreadyExists = _coverExistsSpecification.AlreadyExists(null, null, fileName);

                    if (!alreadyExists)
                    {
                        DownloadAlbumCover(book, cover, DateTime.Now);
                    }
                }
                catch (WebException e)
                {
                    _logger.Warn("Couldn't download media cover for {0}. {1}", book, e.Message);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't download media cover for {0}", book);
                }
            }
        }

        private void DownloadCover(Author author, MediaCover cover, DateTime lastModified)
        {
            var fileName = GetCoverPath(author.Id, MediaCoverEntity.Author, cover.CoverType, cover.Extension);

            _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, author, cover.Url);
            _httpClient.DownloadFile(cover.Url, fileName, USER_AGENT);

            try
            {
                _diskProvider.FileSetLastWriteTime(fileName, lastModified);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Unable to set modified date for {0} image for author {1}", cover.CoverType, author);
            }
        }

        private void DownloadAlbumCover(Book book, MediaCover cover, DateTime lastModified)
        {
            var fileName = GetCoverPath(book.Id, MediaCoverEntity.Book, cover.CoverType, cover.Extension, null);

            _logger.Info("Downloading {0} for {1} {2}", cover.CoverType, book, cover.Url);
            _httpClient.DownloadFile(cover.Url, fileName, USER_AGENT);

            try
            {
                _diskProvider.FileSetLastWriteTime(fileName, lastModified);
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Unable to set modified date for {0} image for book {1}", cover.CoverType, book);
            }
        }

        private void EnsureResizedCovers(Author author, MediaCover cover, bool forceResize, Book book = null)
        {
            int[] heights = GetDefaultHeights(cover.CoverType);

            foreach (var height in heights)
            {
                var mainFileName = GetCoverPath(author.Id, MediaCoverEntity.Author, cover.CoverType, cover.Extension);
                var resizeFileName = GetCoverPath(author.Id, MediaCoverEntity.Author, cover.CoverType, cover.Extension, height);

                if (forceResize || !_diskProvider.FileExists(resizeFileName) || _diskProvider.GetFileSize(resizeFileName) == 0)
                {
                    _logger.Debug("Resizing {0}-{1} for {2}", cover.CoverType, height, author);

                    try
                    {
                        _resizer.Resize(mainFileName, resizeFileName, height);
                    }
                    catch
                    {
                        _logger.Debug("Couldn't resize media cover {0}-{1} for author {2}, using full size image instead.", cover.CoverType, height, author);
                    }
                }
            }
        }

        private int[] GetDefaultHeights(MediaCoverTypes coverType)
        {
            switch (coverType)
            {
                default:
                    return new int[] { };

                case MediaCoverTypes.Poster:
                case MediaCoverTypes.Disc:
                case MediaCoverTypes.Cover:
                case MediaCoverTypes.Logo:
                case MediaCoverTypes.Headshot:
                    return new[] { 500, 250 };

                case MediaCoverTypes.Banner:
                    return new[] { 70, 35 };

                case MediaCoverTypes.Fanart:
                case MediaCoverTypes.Screenshot:
                    return new[] { 360, 180 };
            }
        }

        public void HandleAsync(AuthorRefreshCompleteEvent message)
        {
            EnsureArtistCovers(message.Author);

            var albums = _bookService.GetBooksByAuthor(message.Author.Id);
            foreach (Book book in albums)
            {
                EnsureAlbumCovers(book);
            }

            _eventAggregator.PublishEvent(new MediaCoversUpdatedEvent(message.Author));
        }

        public void HandleAsync(AuthorDeletedEvent message)
        {
            var path = GetArtistCoverPath(message.Author.Id);
            if (_diskProvider.FolderExists(path))
            {
                _diskProvider.DeleteFolder(path, true);
            }
        }

        public void HandleAsync(BookDeletedEvent message)
        {
            var path = GetAlbumCoverPath(message.Book.Id);
            if (_diskProvider.FolderExists(path))
            {
                _diskProvider.DeleteFolder(path, true);
            }
        }
    }
}
