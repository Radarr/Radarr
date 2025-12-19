using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Common
{
    public class ServiceInstallationException : NzbDroneException
    {
        public ServiceInstallationException(string message, params object[] args)
            : base(message, args)
        {
        }

        public ServiceInstallationException(string message)
            : base(message)
        {
        }

        public ServiceInstallationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
