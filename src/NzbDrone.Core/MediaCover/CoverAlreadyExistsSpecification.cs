using System.Threading.Tasks;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.MediaCover
{
    public interface ICoverExistsSpecification
    {
        Task<bool> AlreadyExists(string url, string path);
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

        public async Task<bool> AlreadyExists(string url, string path)
        {
            if (!_diskProvider.FileExists(path))
            {
                return false;
            }

            var response = await _httpClient.HeadAsync(new HttpRequest(url));
            var fileSize = _diskProvider.GetFileSize(path);

            return fileSize == response.Headers.ContentLength;
        }
    }
}
