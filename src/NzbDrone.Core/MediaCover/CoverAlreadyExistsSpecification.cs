using System;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;
using System.Drawing;
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

            if (!IsValidGDIPlusImage(path))
            {
                _diskProvider.DeleteFile(path);
                return false;
            }

            var headers = _httpClient.Head(new HttpRequest(url)).Headers;
            var fileSize = _diskProvider.GetFileSize(path);
            return fileSize == headers.ContentLength;
        }

        private bool IsValidGDIPlusImage(string filename)
        {
            try
            {
                GdiPlusInterop.CheckGdiPlus();

                using (var bmp = new Bitmap(filename))
                {
                }
                return true;
            }
            catch (DllNotFoundException ex)
            {
                _logger.Error(ex, "Could not find libgdiplus. Cannot test if image is corrupt.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex, "Corrupted image found at: {0}. Redownloading...", filename);
                return false;
            }
        }
    }
}