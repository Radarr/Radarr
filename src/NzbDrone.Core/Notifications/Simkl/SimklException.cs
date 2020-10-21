using System;
using NzbDrone.Common.Exceptions;

namespace NzbDrone.Core.Notifications.Simkl
{
    public class SimklException : NzbDroneException
    {
        public SimklException(string message)
            : base(message)
        {
        }

        public SimklException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
