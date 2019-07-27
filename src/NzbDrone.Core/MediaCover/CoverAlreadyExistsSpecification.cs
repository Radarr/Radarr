using System;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Http;

namespace NzbDrone.Core.MediaCover
{
    public interface ICoverExistsSpecification
    {
        bool AlreadyExists(DateTime? serverModifiedDate, long? serverContentLength, string localPath);
    }

    public class CoverAlreadyExistsSpecification : ICoverExistsSpecification
    {
        private readonly IDiskProvider _diskProvider;

        public CoverAlreadyExistsSpecification(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public bool AlreadyExists(DateTime? serverModifiedDate, long? serverContentLength, string localPath)
        {
            if (!_diskProvider.FileExists(localPath))
            {
                return false;
            }

            if (serverModifiedDate.HasValue)
            {
                DateTime? lastModifiedLocal = _diskProvider.FileGetLastWrite(localPath);

                return lastModifiedLocal.Value.ToUniversalTime() == serverModifiedDate.Value.ToUniversalTime();
            }

            if (serverContentLength.HasValue && serverContentLength.Value > 0)
            {
                var fileSize = _diskProvider.GetFileSize(localPath);

                return fileSize == serverContentLength;
            }

            return false;
        }
    }
}
