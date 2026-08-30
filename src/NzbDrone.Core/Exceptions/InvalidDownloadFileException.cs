using System;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Exceptions
{
    public class InvalidDownloadFileException : ReleaseDownloadException
    {
        public InvalidDownloadFileException(ReleaseInfo release, string message, Exception innerException)
            : base(release, message, innerException)
        {
        }
    }
}
