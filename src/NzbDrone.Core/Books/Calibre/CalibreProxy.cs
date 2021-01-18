using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.RemotePathMappings;

namespace NzbDrone.Core.Books.Calibre
{
    public interface ICalibreProxy
    {
        void GetLibraryInfo(CalibreSettings settings);
        CalibreImportJob AddBook(BookFile book, CalibreSettings settings);
        void AddFormat(BookFile file, CalibreSettings settings);
        void RemoveFormats(int calibreId, IEnumerable<string> formats, CalibreSettings settings);
        void SetFields(BookFile file, CalibreSettings settings);
        CalibreBookData GetBookData(int calibreId, CalibreSettings settings);
        long ConvertBook(int calibreId, CalibreConversionOptions options, CalibreSettings settings);
        List<string> GetAllBookFilePaths(CalibreSettings settings);
        CalibreBook GetBook(int calibreId, CalibreSettings settings);
    }

    public class CalibreProxy : ICalibreProxy
    {
        private readonly IHttpClient _httpClient;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly IRemotePathMappingService _pathMapper;
        private readonly Logger _logger;
        private readonly ICached<CalibreBook> _bookCache;

        public CalibreProxy(IHttpClient httpClient,
                            IMapCoversToLocal mediaCoverService,
                            IRemotePathMappingService pathMapper,
                            ICacheManager cacheManager,
                            Logger logger)
        {
            _httpClient = httpClient;
            _mediaCoverService = mediaCoverService;
            _pathMapper = pathMapper;
            _bookCache = cacheManager.GetCache<CalibreBook>(GetType());
            _logger = logger;
        }

        public CalibreImportJob AddBook(BookFile book, CalibreSettings settings)
        {
            var jobid = (int)(DateTime.UtcNow.Ticks % 1000000000);
            var addDuplicates = false;
            var path = book.Path;
            var filename = $"$dummy{Path.GetExtension(path)}";
            var body = File.ReadAllBytes(path);

            _logger.Trace($"Read {body.Length} bytes from {path}");

            try
            {
                var builder = GetBuilder($"cdb/add-book/{jobid}/{addDuplicates}/{filename}", settings);

                var request = builder.Build();
                request.SetContent(body);

                return _httpClient.Post<CalibreImportJob>(request).Resource;
            }
            catch (HttpException ex)
            {
                throw new CalibreException("Unable to add file to calibre library: {0}", ex, ex.Message);
            }
        }

        public void AddFormat(BookFile file, CalibreSettings settings)
        {
            var format = Path.GetExtension(file.Path);
            var bookData = Convert.ToBase64String(File.ReadAllBytes(file.Path));

            var payload = new CalibreChangesPayload
            {
                LoadedBookIds = new List<int> { file.CalibreId },
                Changes = new CalibreChanges
                {
                    AddedFormats = new List<CalibreAddFormat>
                    {
                        new CalibreAddFormat
                        {
                            Ext = format,
                            Data = bookData
                        }
                    }
                }
            };

            ExecuteSetFields(file.CalibreId, payload, settings);
        }

        public void RemoveFormats(int calibreId, IEnumerable<string> formats, CalibreSettings settings)
        {
            var payload = new CalibreChangesPayload
            {
                LoadedBookIds = new List<int> { calibreId },
                Changes = new CalibreChanges
                {
                    RemovedFormats = formats.ToList()
                }
            };

            ExecuteSetFields(calibreId, payload, settings);
        }

        public void SetFields(BookFile file, CalibreSettings settings)
        {
            var edition = file.Edition.Value;
            var book = edition.Book.Value;
            var serieslink = book.SeriesLinks.Value.FirstOrDefault();

            var series = serieslink?.Series.Value;
            double? seriesIndex = null;
            if (double.TryParse(serieslink?.Position, out var index))
            {
                _logger.Trace($"Parsed {serieslink?.Position} as {index}");
                seriesIndex = index;
            }

            _logger.Trace($"Book: {book} Series: {series?.Title}, Position: {seriesIndex}");

            var cover = edition.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Cover);
            string image = null;
            if (cover != null)
            {
                var imageFile = _mediaCoverService.GetCoverPath(edition.BookId, MediaCoverEntity.Book, cover.CoverType, cover.Extension, null);

                if (File.Exists(imageFile))
                {
                    var imageData = File.ReadAllBytes(imageFile);
                    image = Convert.ToBase64String(imageData);
                }
            }

            var payload = new CalibreChangesPayload
            {
                LoadedBookIds = new List<int> { file.CalibreId },
                Changes = new CalibreChanges
                {
                    Title = edition.Title,
                    Authors = new List<string> { file.Author.Value.Name },
                    Cover = image,
                    PubDate = book.ReleaseDate,
                    Publisher = edition.Publisher,
                    Languages = edition.Language,
                    Comments = edition.Overview,
                    Rating = edition.Ratings.Value * 2,
                    Identifiers = new Dictionary<string, string>
                    {
                        { "isbn", edition.Isbn13 },
                        { "asin", edition.Asin },
                        { "goodreads", edition.ForeignEditionId }
                    },
                    Series = series?.Title,
                    SeriesIndex = seriesIndex
                }
            };

            ExecuteSetFields(file.CalibreId, payload, settings);
        }

        private void ExecuteSetFields(int id, CalibreChangesPayload payload, CalibreSettings settings)
        {
            var builder = GetBuilder($"cdb/set-fields/{id}", settings)
                .Post()
                .SetHeader("Content-Type", "application/json");

            var request = builder.Build();
            request.SetContent(payload.ToJson());

            _httpClient.Execute(request);
        }

        public CalibreBookData GetBookData(int calibreId, CalibreSettings settings)
        {
            try
            {
                var builder = GetBuilder($"conversion/book-data/{calibreId}", settings);

                var request = builder.Build();

                return _httpClient.Get<CalibreBookData>(request).Resource;
            }
            catch (HttpException ex)
            {
                throw new CalibreException("Unable to add file to calibre library: {0}", ex, ex.Message);
            }
        }

        public long ConvertBook(int calibreId, CalibreConversionOptions options, CalibreSettings settings)
        {
            try
            {
                var builder = GetBuilder($"conversion/start/{calibreId}", settings);

                var request = builder.Build();
                request.SetContent(options.ToJson());

                var jobId = _httpClient.Post<long>(request).Resource;

                // Run async task to check if conversion complete
                _ = PollConvertStatus(jobId, settings);

                return jobId;
            }
            catch (HttpException ex)
            {
                throw new CalibreException("Unable to start calibre conversion: {0}", ex, ex.Message);
            }
        }

        public CalibreBook GetBook(int calibreId, CalibreSettings settings)
        {
            try
            {
                var builder = GetBuilder($"ajax/book/{calibreId}", settings);

                var request = builder.Build();
                var book = _httpClient.Get<CalibreBook>(request).Resource;

                foreach (var format in book.Formats.Values)
                {
                    format.Path = _pathMapper.RemapRemoteToLocal(settings.Host, new OsPath(format.Path)).FullPath;
                }

                return book;
            }
            catch (HttpException ex)
            {
                throw new CalibreException("Unable to connect to calibre library: {0}", ex, ex.Message);
            }
        }

        public List<string> GetAllBookFilePaths(CalibreSettings settings)
        {
            _bookCache.Clear();

            var ids = GetAllBookIds(settings);
            var result = new List<string>();

            const int count = 100;
            var offset = 0;

            while (offset < ids.Count)
            {
                var builder = GetBuilder($"ajax/books", settings);
                builder.AddQueryParam("ids", ids.Skip(offset).Take(count).ConcatToString(","));

                var request = builder.Build();
                try
                {
                    var response = _httpClient.Get<Dictionary<int, CalibreBook>>(request);
                    foreach (var book in response.Resource.Values)
                    {
                        var remotePath = book?.Formats.Values.OrderBy(f => f.LastModified).FirstOrDefault()?.Path;
                        if (remotePath == null)
                        {
                            continue;
                        }

                        var localPath = _pathMapper.RemapRemoteToLocal(settings.Host, new OsPath(remotePath)).FullPath;
                        result.Add(localPath);

                        _bookCache.Set(localPath, book, TimeSpan.FromMinutes(5));
                    }
                }
                catch (HttpException ex)
                {
                    throw new CalibreException("Unable to connect to calibre library: {0}", ex, ex.Message);
                }

                offset += count;
            }

            return result;
        }

        public List<int> GetAllBookIds(CalibreSettings settings)
        {
            // the magic string is 'allbooks' converted to hex
            var builder = GetBuilder($"/ajax/category/616c6c626f6f6b73", settings);
            const int count = 100;
            var offset = 0;

            var ids = new List<int>();

            while (true)
            {
                var result = GetPaged<CalibreCategory>(builder, count, offset);
                if (!result.Resource.BookIds.Any())
                {
                    break;
                }

                offset += count;
                ids.AddRange(result.Resource.BookIds);
            }

            return ids;
        }

        private HttpResponse<T> GetPaged<T>(HttpRequestBuilder builder, int count, int offset)
            where T : new()
        {
            builder.AddQueryParam("num", count, replace: true);
            builder.AddQueryParam("offset", offset, replace: true);

            var request = builder.Build();

            try
            {
                return _httpClient.Get<T>(request);
            }
            catch (HttpException ex)
            {
                throw new CalibreException("Unable to connect to calibre library: {0}", ex, ex.Message);
            }
        }

        public void GetLibraryInfo(CalibreSettings settings)
        {
            try
            {
                var builder = GetBuilder($"ajax/library-info", settings);
                var request = builder.Build();
                var response = _httpClient.Execute(request);
            }
            catch (HttpException ex)
            {
                throw new CalibreException("Unable to connect to calibre library: {0}", ex, ex.Message);
            }
        }

        private HttpRequestBuilder GetBuilder(string relativePath, CalibreSettings settings)
        {
            var baseUrl = HttpRequestBuilder.BuildBaseUrl(settings.UseSsl, settings.Host, settings.Port, settings.UrlBase);
            baseUrl = HttpUri.CombinePath(baseUrl, relativePath);

            var builder = new HttpRequestBuilder(baseUrl)
                .Accept(HttpAccept.Json);

            builder.LogResponseContent = true;

            if (settings.Username.IsNotNullOrWhiteSpace())
            {
                builder.NetworkCredential = new NetworkCredential(settings.Username, settings.Password);
            }

            return builder;
        }

        private async Task PollConvertStatus(long jobId, CalibreSettings settings)
        {
            var builder = GetBuilder($"/conversion/status/{jobId}", settings);
            var request = builder.Build();

            while (true)
            {
                var status = _httpClient.Get<CalibreConversionStatus>(request).Resource;

                if (!status.Running)
                {
                    if (!status.Ok)
                    {
                        _logger.Warn("Calibre conversion failed.\n{0}\n{1}", status.Traceback, status.Log);
                    }

                    return;
                }

                await Task.Delay(2000);
            }
        }
    }
}
