using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using NLog;

namespace NzbDrone.Core.MediaCover
{
    public interface ICoverExistsSpecification
    {
        bool AlreadyExists(string url, string path);
    }

    public class CoverAlreadyExistsSpecification : ICoverExistsSpecification
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public CoverAlreadyExistsSpecification(IDiskProvider diskProvider, IHttpClient httpClient, Logger logger)
        {
            _diskProvider = diskProvider;
            _httpClient = httpClient;
            _logger = logger;
        }

        public bool AlreadyExists(string url, string path)
        {
            if (!_diskProvider.FileExists(path))
            {
                return false;
            }

            var headers = _httpClient.Head(new HttpRequest(url)).Headers;
            var fileSize = _diskProvider.GetFileSize(path);
            return fileSize == headers.ContentLength;
        }
    }
}
