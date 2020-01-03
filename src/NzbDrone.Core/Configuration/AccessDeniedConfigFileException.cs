using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Configuration
{
    public class AccessDeniedConfigFileException : NzbDroneException
    {
        public AccessDeniedConfigFileException(string message)
            : base(message)
        {
        }

        public AccessDeniedConfigFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
