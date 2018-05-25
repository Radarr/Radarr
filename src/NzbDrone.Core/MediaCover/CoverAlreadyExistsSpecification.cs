using System;
using NzbDrone.Common.Disk;

namespace NzbDrone.Core.MediaCover
{
    public interface ICoverExistsSpecification
    {
        bool AlreadyExists(DateTime serverModifiedDate, string localPath);
    }

    public class CoverAlreadyExistsSpecification : ICoverExistsSpecification
    {
        private readonly IDiskProvider _diskProvider;

        public CoverAlreadyExistsSpecification(IDiskProvider diskProvider)
        {
            _diskProvider = diskProvider;
        }

        public bool AlreadyExists(DateTime lastModifiedDateServer, string localPath)
        {
            if (!_diskProvider.FileExists(localPath))
            {
                return false;
            }

            DateTime? lastModifiedLocal = _diskProvider.FileGetLastWrite(localPath);

            return lastModifiedLocal.Value.ToUniversalTime() == lastModifiedDateServer.ToUniversalTime();
        }
    }
}
